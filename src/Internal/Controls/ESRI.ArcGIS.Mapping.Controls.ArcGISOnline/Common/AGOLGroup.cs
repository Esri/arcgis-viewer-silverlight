/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Provides methods to interact with groups on ArcGIS Online.
  /// </summary>
  public class AGOLGroup
  {
    ArcGISOnline _agol;

    public AGOLGroup(ArcGISOnline agol)
    {
      _agol = agol;
    }

    /// <summary>
    /// Adds a new group to AGOL.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="keywords"></param>
    /// <param name="summary"></param>
    /// <param name="thumbnail"></param>
    /// <param name="isPublic">If true this group will be accessible publicly</param>
    /// <param name="isInvitationOnly">If true this group will not accept join requests. Only admins can invite users to the group.</param>
    /// <param name="callback"></param>
    public void AddGroup(string title, string keywords, string summary, Stream thumbnail, bool isPublic, bool isInvitationOnly, EventHandler<RequestEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "title", Value = title });
      items.Add(new HttpContentItem() { Name = "tags", Value = keywords });
      items.Add(new HttpContentItem() { Name = "snippet", Value = summary });
      items.Add(new HttpContentItem() { Name = "thumbnail", Type = "image/x-png", Filename = "thumbnail.png", Value = thumbnail });
      items.Add(new HttpContentItem() { Name = "isPublic", Value = isPublic });
      items.Add(new HttpContentItem() { Name = "isInvitationOnly", Value = isInvitationOnly });
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/createGroup?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Group result = WebUtil.ReadObject<Group>(e.Result);

          if (result != null && result.Id != null && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
			  callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToCreateGroup) });
        });
    }

    /// <summary>
    /// Performs a search for groups using the specified query.
    /// </summary>
    public void Search(string query, EventHandler<GroupSearchEventArgs> callback)
    {
      string url = _agol.Url + "community/groups?q=" + query + "&sortField=title&f=json&num=12";
      if (_agol.User.Current != null)
        url += "&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
          SearchCompleted(e, callback);
        });
    }

    /// <summary>
    /// Performs a search for groups using the specified query and startIndex.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="startIndex">The index of the starting result for paging.</param>
    /// <param name="callback"></param>
    public void Search(string query, int startIndex, int maxCount, EventHandler<GroupSearchEventArgs> callback)
    {
      string url = _agol.Url + "community/groups?q=" + query + "&sortField=title&f=json&start=" + startIndex.ToString() + "&num=" + maxCount.ToString();
      if (_agol.User.Current != null)
        url += "&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
          SearchCompleted(e, callback);
        });
    }

    /// <summary>
    /// Called when a search for groups completes.
    /// Initializes the thumbnails of the groups and invokes the callback.
    /// </summary>
    void SearchCompleted(OpenReadEventArgs e, EventHandler<GroupSearchEventArgs> callback)
    {
      if (e.Error != null)
      {
        if (callback != null)
          callback(null, new GroupSearchEventArgs() { Error = e.Error });
        return;
      }

      GroupSearchResult result = WebUtil.ReadObject<GroupSearchResult>(e.Result);

      if (result.Items != null)
      {
          // setup the thumbnails for each result
          //
          foreach (Group group in result.Items)
              if (group.ThumbnailPath != null)
              {
                  string thumbnailUrl = _agol.Url + "community/groups/" + group.Id + "/info/" + group.ThumbnailPath;
                  if (_agol.User.Current != null)
                      thumbnailUrl += "?token=" + _agol.User.Token;

                  group.Thumbnail = new BitmapImage(new Uri(thumbnailUrl));
              }
      }

      if (callback != null)
        callback(null, new GroupSearchEventArgs() { Result = result });
    }

    /// <summary>
    /// Gets the members of the specified group.
    /// </summary>
    public void GetMembers(string groupId, EventHandler<GroupMembersEventArgs> callback)
    {
      string url = _agol.Url + "community/groups/" + groupId + "/users?f=json";
      if (_agol.User.Current != null)
        url += "&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
          if (e.Error != null)
          {
            if (callback != null)
              callback(this, new GroupMembersEventArgs() { Error = e.Error });
            return;
          }

          GroupMembers result = WebUtil.ReadObject<GroupMembers>(e.Result);

          if (callback != null)
            callback(null, new GroupMembersEventArgs() { GroupMembers = result });
        });
    }

    /// <summary>
    /// Features the specified map in the specified group. 
    /// </summary>
    public void FeatureMap(string groupId, string mapId, EventHandler<RequestEventArgs> callback)
    {
      //get the group to get to the FeaturedItemsId
      GetGroup(groupId, (object sender, GroupEventArgs e) =>
        {
          if (e.Error != null)
          {
            if (callback != null)
              callback(null, new RequestEventArgs() { Error = e.Error });
            return;
          }

          //try to retrieve the Featured Items item from the group
          _agol.Content.GetItem(e.Group.FeaturedItemsId, (object sender2, ContentItemEventArgs e2) =>
            {
              if (e2.Error != null || e2.Item == null)
              {
                //getting the Featured Items did not succeed
                //create a new Featured Items item and associate it with the group
                CreateFeaturedItemItemForGroup(e.Group, (object sender3, ContentItemEventArgs e3) =>
                {
                  if (e3.Error != null)
                  {
                    if (callback != null)
                      callback(null, new RequestEventArgs() { Error = e3.Error });
                    return;
                  }

                  //add a relationship of type FeaturedItems2Item between the Featured Items item and the map
                  _agol.Content.AddRelationship(e3.Item.Id, mapId, "FeaturedItems2Item", callback);
                });
                return;
              }

              //getting the Featured Items item succeeded
              //add a relationship of type FeaturedItems2Item between the Featured Items item and the map
              _agol.Content.AddRelationship(e2.Item.Id, mapId, "FeaturedItems2Item", callback);
            });
        });
    }

    /// <summary>
    /// Creates a "Featured Items" text item for the specified group, associates it with the group and shares it.
    /// </summary>
    void CreateFeaturedItemItemForGroup(Group group, EventHandler<ContentItemEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "text", Value = "Featured Items of Group " + group.Id });
      items.Add(new HttpContentItem() { Name = "type", Value = "Featured Items" });
      items.Add(new HttpContentItem() { Name = "item", Value = "GroupFeaturedItems_" + group.Id });
      items.Add(new HttpContentItem() { Name = "overwrite", Value = "true" });
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      //store the Featured Items item on AGOL
      string url = _agol.Url + "content/users/" + _agol.User.Current.Username + "/addItem?f=json&token=" + _agol.User.Token;
      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          ContentItem item = WebUtil.ReadObject<ContentItem>(e.Result);

          //get the actual ContentItem
          if (item.Id != null)
            _agol.Content.GetItem(item.Id, (object sender2, ContentItemEventArgs e2) =>
              {
                if (e2.Error != null)
                {
                  if (callback != null)
                    callback(null, new ContentItemEventArgs() { Error = e2.Error });
                  return;
                }

                //set the item's id into the group's featuredItemsId property
                Update(group.Id, "featuredItemsId", e2.Item.Id, (object sender3, RequestEventArgs e3) =>
                {
                  if (e3.Error != null)
                  {
                    if (callback != null)
                      callback(null, new ContentItemEventArgs() { Error = e3.Error });
                    return;
                  }

                  //group has been updated successfully
                  //now share the Featured Items item to the group and as public or non public depending if the
                  //group is public or non public
                  string[] groups = { group.Id };
                  _agol.Content.ShareItem(e2.Item, group.IsPublic, groups, (object sender4, RequestEventArgs e4) =>
                    {
                      if (callback != null)
                        callback(null, new ContentItemEventArgs() { Error = e4.Error, Item = e2.Item });
                    });
                });
              });
        });
    }

    /// <summary>
    /// Updates a group's property with the specified value.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="paramName">A property of the group.</param>
    /// <param name="value">The value of the property.</param>
    /// <param name="callback"></param>
    public void Update(string groupId, string paramName, object value, EventHandler<RequestEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = paramName, Value = value });
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/update?token=" + _agol.User.Token + "&clearEmptyFields=true";

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToUpdateGroup, groupId)) });
        });
    }

    /// <summary>
    /// Updates a group's properties with the specified values.
    /// </summary>
    /// <remarks>The order of the values has to match the order of the paramter names.</remarks>
    /// <param name="groupId"></param>
    /// <param name="paramName">A collection of property names of the group.</param>
    /// <param name="value">The values of the properties.</param>
    /// <param name="callback"></param>
    public void Update(string groupId, string[] paramNames, object[] values, EventHandler<RequestEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();

      int index = 0;
      foreach (string paramName in paramNames)
      {
        items.Add(new HttpContentItem() { Name = paramName, Value = values[index] });
        index++;
      }
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/update?token=" + _agol.User.Token + "&clearEmptyFields=true";

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToUpdateGroup, groupId)) });
        });
    }

     /// <summary>
    /// Updates the thumbnail of a group.
    /// </summary>
    public void Update(string groupId, Stream thumbnail, EventHandler<RequestEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "thumbnail", Type = "image/x-png", Filename = "thumbnail.png", Value = thumbnail });
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/update?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
      {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
			  callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToUpdateGroup, groupId)) });
        });
    }

    /// <summary>
    /// Removes a map from the list of featured maps of the specified group.
    /// </summary>
    public void UnfeatureMap(string groupId, string mapId, EventHandler<RequestEventArgs> callback)
    {
      //get the group
      _agol.Group.GetGroup(groupId, (object sender, GroupEventArgs e) =>
        {
          if (e.Error != null)
          {
            if (callback != null || e.Group == null)
              callback(null, new RequestEventArgs() { Error = e.Error });
            return;
          }

          //remove the relationship between the group's Featured Items item and the map
          _agol.Content.RemoveRelationship(e.Group.FeaturedItemsId, mapId, "FeaturedItems2Item", callback);
        });
    }

    /// <summary>
    /// Retrieves the group specified by its Id.
    /// </summary>
    public void GetGroup(string groupId, EventHandler<GroupEventArgs> callback)
    {
      string url = _agol.Url + "community/groups/" + groupId + "?f=json";
      if (_agol.User.Current != null)
        url += "&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
          if (e.Error != null)
          {
            if(callback != null)
              callback(null, new GroupEventArgs() { Error = e.Error });
            return;
          }

          Group group = WebUtil.ReadObject<Group>(e.Result);

          // setup the thumbnails for the group
          //
          if (group.ThumbnailPath != null)
          {
            string thumbnailUrl = _agol.Url + "community/groups/" + group.Id + "/info/" + group.ThumbnailPath + "?tickCount=" + Environment.TickCount.ToString(); ;
            if (_agol.User.Current != null)
              thumbnailUrl += "&token=" + _agol.User.Token;

            group.Thumbnail = new BitmapImage(new Uri(thumbnailUrl));
          }

          if(callback != null)
            callback(null, new GroupEventArgs() { Group = group });
        });
    }

    /// <summary>
    /// Deltes the group specified by its Id.
    /// </summary>
    public void Delete(string groupId, EventHandler<RequestEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/delete?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
			  callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToDeleteGroup, groupId)) });
        });
    }

    /// <summary>
    /// Returns the url of a group on AGOL.
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public string GetGroupUrl(string groupId)
    {
      string url = _agol.Url + "community/groups/" + groupId;

      if (_agol.User.Current != null)
        url += "?token=" + _agol.User.Token;

      return url;
    }

    /// <summary>
    /// Gets the join applications for the specified group.
    /// </summary>
    public void GetApplications(string groupId, EventHandler<GroupApplicationEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new GroupApplicationEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }
   
      string url = _agol.Url + "community/groups/" + groupId + "/applications?f=json&token=" + _agol.User.Token;
      
      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
      {
        if (e.Error != null)
        {
          if (callback != null)
            callback(null, new GroupApplicationEventArgs() { Error = e.Error });
          return;
        }

        GroupApplications result = WebUtil.ReadObject<GroupApplications>(e.Result);

        if (callback != null && result != null && result.Applications != null)
          callback(null, new GroupApplicationEventArgs() { Applications = result.Applications });
        else if(callback != null)
			callback(null, new GroupApplicationEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToGetGroupApplicationsForGroup, groupId)) });
      });
    }

    /// <summary>
    /// Accept the application of a user to join the specified group.
    /// </summary>
    /// <param name="groupId">The group the user wants to join.</param>
    /// <param name="applicant">The user that want to join the group.</param>
    /// <param name="callback"></param>
    public void AcceptApplication(string groupId, string applicant, EventHandler<RequestEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/applications/" + applicant + "/accept?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToGetGroupApplicationsForGroup, applicant, groupId)) });
        });
    }

    /// <summary>
    /// Decline the application of a user to join the specified group.
    /// </summary>
    /// <param name="groupId">The group the user wants to join.</param>
    /// <param name="applicant">The user that want to join the group.</param>
    /// <param name="callback"></param>
    public void DeclineApplication(string groupId, string applicant, EventHandler<RequestEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/applications/" + applicant + "/decline?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFaileDeclineApplicateForGroup, applicant, groupId)) });
        });
    }

    /// <summary>
    /// Sends invitations to the specified users to join the specified group.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="users">A comma separated list of usernames to be invited to the group. 
    /// If one of the users is a member of the group already or an invitation has already been 
    /// sent the call still returns a success.</param>
    /// <param name="callback"></param>
    public void Invite(string groupId, string users, EventHandler<RequestEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "users", Value = users });
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/invite?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedInviteUserToGroup, users, groupId)) });
        });
    }

    /// <summary>
    /// Sends a request to leave the specified group.
    /// </summary>
    public void Leave(string groupId, EventHandler<RequestEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/leave?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedLeaveRequestForGroup, _agol.User.Current.Username, groupId)) });
        });
    }

    /// <summary>
    /// Sends a request to join the specified group.
    /// </summary>
    public void Join(string groupId, EventHandler<RequestEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/groups/" + groupId + "/join?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedSendJoinRequestToGroup, _agol.User.Current.Username, groupId)) });
        });
    }
  }
}
