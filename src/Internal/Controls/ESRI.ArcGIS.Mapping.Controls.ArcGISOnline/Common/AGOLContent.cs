/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Provides methods to interact with content on ArcGIS Online.
  /// </summary>
  public class AGOLContent
  {
    ArcGISOnline _agol;

    public AGOLContent(ArcGISOnline agol)
    {
      _agol = agol;
    }

    /// <summary>
    /// Raised when a map has been deleted.
    /// </summary>
    public event EventHandler<ContentItemEventArgs> ItemDeleted;

    /// <summary>
    /// Performs a search for content using the specified query.
    /// </summary>
    public void Search(string query, EventHandler<ContentSearchEventArgs> callback)
    {
      Search(query, "&sortField=title", callback);
    }

    /// <summary>
    /// Performs a search for content using the specified query.
    /// </summary>
    public void Search(string query, string sort, EventHandler<ContentSearchEventArgs> callback)
    {
      string url = _agol.Url + "search?q=" + query + sort + "&f=json&num=12";
      if (_agol.User.Current != null)
        url += "&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
          SearchCompleted(e, sort, callback);
        });
    }

    /// <summary>
    /// Performs a search for content using the specified query and startIndex.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="startIndex">The index of the starting result for paging.</param>
    /// <param name="maxCount"></param>
    /// <param name="callback"></param>
    public void Search(string query, string sort, int startIndex, int maxCount, EventHandler<ContentSearchEventArgs> callback)
    {
      string url = _agol.Url + "search?q=" + query + sort + "&f=json&start=" + startIndex.ToString() + "&num=" + maxCount.ToString();
      if (_agol.User.Current != null)
        url += "&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, callback, (sender, e) =>
        {
          SearchCompleted(e, sort, callback);
        });
    }

    /// <summary>
    /// Called when a search for content completes.
    /// Initializes the thumbnails and invokes the callback.
    /// </summary>
    void SearchCompleted(OpenReadEventArgs e, string sort, EventHandler<ContentSearchEventArgs> callback)
    {
      if (e.Error != null)
      {
        if (callback != null)
          callback(this, new ContentSearchEventArgs() { Error = e.Error });
        return;
      }

      ContentSearchResult result = WebUtil.ReadObject<ContentSearchResult>(e.Result);
      result.Sort = sort;

      // setup the thumbnails for each result
      //
      if (result != null && result.Items != null)
      {
        foreach (ContentItem item in result.Items)
          if (item.ThumbnailPath != null)
          {
            string thumbnailUrl = _agol.Url + "content/items/" + item.Id + "/info/" + item.ThumbnailPath + "?tickCount=" + Environment.TickCount.ToString();
            if (_agol.User.Current != null)
              thumbnailUrl += "&token=" + _agol.User.Token;

            item.Thumbnail = new BitmapImage(new Uri(thumbnailUrl));
          }
      }

      if (callback != null)
        callback(null, new ContentSearchEventArgs() { Result = result });
    }


    /// <summary>
    /// Retrieves the ContentItem specified by its Id.
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="useCachedThumbnail">
    /// Specifies if the thumbnail from the cache can be 
    /// reused if the thumbnail is already cached.
    /// </param>
    /// <param name="callback"></param>
    public void GetItem(string itemId, EventHandler<ContentItemEventArgs> callback)
    {
      string url = _agol.Url + "content/items/" + itemId + "?f=json";
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
              callback(this, new ContentItemEventArgs() { Error = e.Error, Id = itemId });
            return;
          }

          ContentItem item = WebUtil.ReadObject<ContentItem>(e.Result);

          // did the request succeed with a valid item?
          //
          if (item.Id == null)
          {
            if (callback != null)
              callback(this, new ContentItemEventArgs() { Error = new Exception("Invalid identifier or access denied."), Id = itemId });
            return;
          }

          // setup the thumbnail
          if (item.ThumbnailPath != null)
          {
            string thumbnailUrl = _agol.Url + "content/items/" + item.Id + "/info/" + item.ThumbnailPath + "?tickCount=" + Environment.TickCount.ToString();
            if (_agol.User.Current != null)
              thumbnailUrl += "&token=" + _agol.User.Token;

            item.Thumbnail = new BitmapImage(new Uri(thumbnailUrl));
          }

          if (callback != null)
            callback(this, new ContentItemEventArgs() { Item = item });
        });
    }

      /// <summary>
    /// Adds a relationship of the specified type between two items specified by their Id.
    /// </summary>
    /// <remarks>
    /// Relationships are not tied to an item. They are directional links from an origin item to 
    /// a destination item. They have an owner and a type. The type defines the valid origin and 
    /// destination item types as well as some 'composite' rules.
    /// </remarks>
    /// <param name="originItemId">The item id of the 'origin' item of the relationship.</param>
    /// <param name="destinationItemId">The item id of the 'destination' item of the relationship.</param>
    /// <param name="relationshipType">The type of relationship between the two items.</param>
    /// <param name="callback"></param>
    public void AddRelationship(string originItemId, string destinationItemId, string relationshipType, EventHandler<RequestEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "originItemId", Value = originItemId });
      items.Add(new HttpContentItem() { Name = "destinationItemId", Value = destinationItemId });
      items.Add(new HttpContentItem() { Name = "relationshipType", Value = relationshipType });
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "content/users/" + _agol.User.Current.Username + "/addRelationship?token=" + _agol.User.Token;
      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && callback != null)
            callback(null, new RequestEventArgs() { Error = result.Succeeded ? null : new Exception("Failed to add feature relationship.") });
          else
            callback(null, new RequestEventArgs() { Error = new Exception("Failed to add feature relationship.") });
        });
    }

    /// <summary>
    /// Removes a relationship of the specified type between the specified origin and destination items.
    /// </summary>
    /// <remarks>
    /// The current user must own the relationship to delete it.
    /// </remarks>
    /// <param name="originItemId">The item id of the 'origin' item of the relationship.</param>
    /// <param name="destinationItemId">The item id of the 'destination' item of the relationship.</param>
    /// <param name="relationshipType">The type of relationship between the two items.</param>
    /// <param name="callback"></param>
    public void RemoveRelationship(string originItemId, string destinationItemId, string relationshipType, EventHandler<RequestEventArgs> callback)
    {
      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "originItemId", Value = originItemId });
      items.Add(new HttpContentItem() { Name = "destinationItemId", Value = destinationItemId });
      items.Add(new HttpContentItem() { Name = "relationshipType", Value = relationshipType });
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "content/users/" + _agol.User.Current.Username + "/deleteRelationship?token=" + _agol.User.Token;
      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null)
            callback(null, new RequestEventArgs() { Error = (result.Succeeded) ? null : new Exception("Failed to remove relationship.") });
          else
            callback(null, new RequestEventArgs() { Error = new Exception("Failed to remove relationship.") });
        });
    }

    /// <summary>
    /// Shares the specified item with the specified groups and sets the overall access level (everyone). 
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="everyone">Overall access level - everyone or just available to the owner.</param>
    /// <param name="groupIds">A collection of group Ids which the specified item is shared with.</param>
    /// <param name="callback"></param>
    public void ShareItem(ContentItem item, bool everyone, string[] groupIds, EventHandler<RequestEventArgs> callback)
    {
      //find out if the item is located in a folder
      //
      GetFolder(item, (object sender, RequestEventArgs e) =>
        {
          if (e.Error != null)
          {
            if (callback != null)
              callback(null, e);
            return;
          }

          List<HttpContentItem> items = new List<HttpContentItem>();
          items.Add(new HttpContentItem() { Name = "everyone", Value = everyone });
          items.Add(new HttpContentItem() { Name = "f", Value = "json" });

          string groups = "";
          if (groupIds != null)
            foreach (string groupId in groupIds)
              groups += groupId + ", ";
          items.Add(new HttpContentItem() { Name = "groups", Value = groups });

          string folderUrl = ContentFolder.IsSubfolder(item.Folder) ? "/" + item.Folder.Id : "";
          string url = _agol.Url + "content/users/" + _agol.User.Current.Username + folderUrl + "/items/" + item.Id + "/share?token=" + _agol.User.Token;
          WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender2, e2) =>
            {
              ShareItemResult result = WebUtil.ReadObject<ShareItemResult>(e2.Result);

              if (result != null)
                callback(null, new RequestEventArgs());
              else
                callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToShareItem, item.Id)) });
            });
        });
    }

    /// <summary>
    /// Unshares the specified item from the specified groups. 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="groupIds">A collection of group Ids which the specified item is unshared from.</param>
    /// <param name="callback"></param>
    public void UnshareItem(ContentItem item, string[] groupIds, EventHandler<RequestEventArgs> callback)
    {
      //find out if the item is located in a folder
      //
      GetFolder(item, (object sender, RequestEventArgs e) =>
        {
          if (e.Error != null)
          {
            if (callback != null)
              callback(null, e);
            return;
          }

          List<HttpContentItem> items = new List<HttpContentItem>();
          items.Add(new HttpContentItem() { Name = "f", Value = "json" });

          string groups = "";
          if (groupIds != null)
            foreach (string groupId in groupIds)
              groups += groupId + ", ";
          items.Add(new HttpContentItem() { Name = "groups", Value = groups });

          string folderUrl = ContentFolder.IsSubfolder(item.Folder) ? "/" + item.Folder.Id : "";
          string url = _agol.Url + "content/users/" + _agol.User.Current.Username + folderUrl + "/items/" + item.Id + "/unshare?token=" + _agol.User.Token;
          WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender2, e2) =>
            {
              UnshareItemResult result = WebUtil.ReadObject<UnshareItemResult>(e2.Result);

              if (callback != null)
              {
                if (result != null)
                  callback(null, new RequestEventArgs());
                else
                  callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToShareItem, item.Id)) });
              }
            });
        });
    }

    /// <summary>
    /// Get all the related items of a certain relationship type for that item. 
    /// An optional direction can be specified if the direction of the relationship is ambiguous. Otherwise the 
    /// service will try to infer it.
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="relationshipType">The type of relationship between the two items.</param>
    /// <param name="direction">The direction of the relationship. Either 'forward' (from origin -> destination) or 'reverse' (from destination -> origin). </param>
    /// <param name="callback"></param>
    public void GetRelatedItems(string itemId, string relationshipType, string direction, EventHandler<ContentItemsEventArgs> callback)
    {
      string url = _agol.Url + "content/items/" + itemId + "/relatedItems?relationshipType=" + relationshipType + "&direction=" + direction + "&f=json";
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
              callback(null, new ContentItemsEventArgs() { Error = e.Error });
            return;
          }

          RelatedItemsResult result = WebUtil.ReadObject<RelatedItemsResult>(e.Result);

          if (result != null && result.RelatedItems != null)
          {
            // setup the thumbnails for each result
            //
            foreach (ContentItem item in result.RelatedItems)
              if (item.ThumbnailPath != null)
              {
                string thumbnailUrl = _agol.Url + "content/items/" + item.Id + "/info/" + item.ThumbnailPath;
                if (_agol.User.Current != null)
                  thumbnailUrl += "?token=" + _agol.User.Token;

                item.Thumbnail = new BitmapImage(new Uri(thumbnailUrl));
              }
          }

          if (callback != null)
            callback(null, new ContentItemsEventArgs() { Items = (result == null) ? null : result.RelatedItems });
        });
    }

    /// <summary>
    /// Gets the sharing information for the specified ContentItem which includes the groups 
    /// the item is shared with and overall access permission (public/non public). 
    /// </summary>
    public void GetSharingInfo(ContentItem item, EventHandler<SharingInfoEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
          callback(null, new SharingInfoEventArgs());
        return;
      }

      //find out if the item is located in a folder
      //
      GetFolder(item, (object sender, RequestEventArgs e) =>
        {
          if (e.Error != null)
          {
            if (callback != null)
              callback(null, new SharingInfoEventArgs() { Error = e.Error });
            return;
          }

          string folderUrl = ContentFolder.IsSubfolder(item.Folder) ? "/" + item.Folder.Id : "";
          string url = _agol.Url + "content/users/" + _agol.User.Current.Username + folderUrl + "/items/" + item.Id + "?f=json&token=" + _agol.User.Token;

          // add a bogus parameter to avoid the caching that happens with the WebClient
          //
          url += "&tickCount=" + Environment.TickCount.ToString();

          WebUtil.OpenReadAsync(url, null, (sender2, e2) =>
            {
              if (e2.Error != null)
              {
                if (callback != null)
                  callback(null, new SharingInfoEventArgs() { Error = e2.Error });
                return;
              }

              UserItem userItem = WebUtil.ReadObject<UserItem>(e2.Result);

              if (callback != null)
                callback(null, new SharingInfoEventArgs() { SharingInfo = (item == null) ? null : userItem.Sharing, Item = (item == null) ? null : userItem.ContentItem });
            });
        });
    }

    /// <summary>
    /// Deletes the content item specified by its Id.
    /// </summary>
    public void Delete(ContentItem item, EventHandler<ContentItemEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new ContentItemEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionDeleteFailedUserSignIn), Id = item.Id });
        return;
      }

      //find out if the item is located in a folder
      //
      GetFolder(item, (object sender, RequestEventArgs e) =>
        {
          if (e.Error != null)
          {
            if (callback != null)
              callback(null, new ContentItemEventArgs() { Error = e.Error });
            return;
          }

          string folderUrl = ContentFolder.IsSubfolder(item.Folder) ? "/" + item.Folder.Id : "";
          string url = _agol.Url + "/content/users/" + _agol.User.Current.Username + folderUrl + "/items/" + item.Id + "/delete?f=json&token=" + _agol.User.Token;

          WebUtil.UploadStringAsync(url, null, "", (object sender2, UploadStringCompletedEventArgs e2) =>
            {
              if (e2.Error != null)
              {
                if (callback != null)
                  callback(null, new ContentItemEventArgs() { Error = e2.Error, Id = item.Id });
                return;
              }

              Success result = WebUtil.ReadObject<Success>(new MemoryStream(Encoding.UTF8.GetBytes(e2.Result)));

			  ContentItemEventArgs eventArgs = new ContentItemEventArgs() { Error = (result == null || !result.Succeeded) ? new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionDeleteFailed) : null, Id = item.Id };
              if (callback != null)
                callback(null, eventArgs);

              //raise event
              if (ItemDeleted != null)
                ItemDeleted(null, eventArgs);
            });
        });
    }

    /// <summary>
    /// Retrieves the comments for the specified content item.
    /// </summary>
    public void GetComments(string itemId, EventHandler<CommentEventArgs> callback)
    {
      string url = _agol.Url + "content/items/" + itemId + "/comments?f=json";

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
            callback(this, new CommentEventArgs() { Error = e.Error });
          return;
        }

        ItemComments result = WebUtil.ReadObject<ItemComments>(e.Result);

        if (callback != null)
        {
          if (result != null)
            callback(null, new CommentEventArgs() { Comments = result.Comments });
          else
			  callback(null, new CommentEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionGetCommentsFailed) });
        }
      });
    }

   /// <summary>
    /// Returns the url of a ContentItem on AGOL.
    /// </summary>
    public string GetItemUrl(string itemId)
    {
		string agolRoot = _agol.Url.ToLower().Replace("sharing/", "home/item.html?id=");
		string url = agolRoot + itemId;
      if (_agol.User.Current != null)
        url += "?token=" + _agol.User.Token;

      return url;
    }

    /// <summary>
    /// Gets the folder of a content item if the item is in a folder.
    /// </summary>
    void GetFolder(ContentItem item, EventHandler<RequestEventArgs> callback)
    {
      if (_agol.User.Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionGetFolderFailedUserSignedIn) });
        return;
      }

      //if the folder has already been determined call back immediately
      //
      if (item.Folder != null)
      {
        callback(null, new RequestEventArgs());
        return;
      }

      //get the user's content and find the item
      //
      GetUserContent(null, (object sender, UserContentEventArgs e) =>
        {
          if (e.Error != null)
          {
            callback(null, new RequestEventArgs() { Error = e.Error });
            return;
          }

          UserContent userContent = e.Content;
          if (userContent == null)
          {
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ItemNotFound, item.Id)) });
            return;
          }

          //first check if the item is in the content root level
          //
          if (userContent.ContentItems != null)
          {
            foreach (ContentItem contentItem in userContent.ContentItems)
              if (contentItem.Id == item.Id)
              {
                //create a folder that represents the root folder by assigning an empty id
                item.Folder = new ContentFolder() { Id = "" };

                callback(null, new RequestEventArgs());
                return;
              }
          }

          //search folders for the content item
          if (userContent.Folders == null || userContent.Folders.Length == 0)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ItemNotFound, item.Id)) });
          else
          {
            bool folderFound = false;
            int foldersToSearch = 0;
            int foldersSearched = 0;
            foreach (ContentFolder folder in userContent.Folders)
            {
              foldersToSearch++;
              ContentFolder folderToSearch = folder; //cache for reference in lambda expression
              GetUserContent(folderToSearch.Id, (object sender2, UserContentEventArgs e2) =>
                {
                  foldersSearched++;

                  if (e2.Content != null && e2.Content.ContentItems != null)
                    foreach (ContentItem contentItem in e2.Content.ContentItems)
                      if (contentItem.Id == item.Id)
                      {
                        folderFound = true; //make sure callback is not invoked on subsequent responses

                        item.Folder = folderToSearch;

                        callback(null, new RequestEventArgs());
                        return;
                      }

                  if (foldersSearched == foldersToSearch && !folderFound)
                  {
                    callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ItemNotFound,  item.Id)) });
                    return;
                  }
                });
            }
          }
        });
    }

    /// <summary>
    /// Gets the content of the current user.
    /// <remarks>
    /// If folderId is null gets the content from the root level, otherwise the content in the specified folder.
    /// </remarks>
    /// </summary>
    /// <param name="folderId">The id of the folder to retrive content from. If null the root level content is retrieved.</param>
    /// <param name="callback"></param>
    void GetUserContent(string folderId, EventHandler<UserContentEventArgs> callback)
    {
      string url = _agol.Url + "content/users/" + _agol.User.Current.Username + (string.IsNullOrEmpty(folderId) ? "" : "/" + folderId) + "?f=json&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
          if (e.Error != null)
          {
            callback(null, new UserContentEventArgs() { Error = e.Error });
            return;
          }

          UserContent userContent = WebUtil.ReadObject<UserContent>(e.Result);
          callback(null, new UserContentEventArgs() { Content = userContent });
        });
    }
  }
}
