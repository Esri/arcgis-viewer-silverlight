/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Windows.Media.Imaging;
using System.Runtime.Serialization.Json;
using System.Windows.Browser;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Mapping.Core;
using System.Linq;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Provides methods to interact with users on ArcGIS Online.
  /// </summary>
  public class AGOLUser : INotifyPropertyChanged
  {
    ArcGISOnline _agol;

    public AGOLUser(ArcGISOnline agol)
    {
      _agol = agol;
    }

    /// <summary>
    /// Raised when the user signs in or signs out.
    /// </summary>
    public event EventHandler SignedInOut;

    private User _current;
    /// <summary>
    /// Returns the currently signed in user.
    /// </summary>
    public User Current 
    {
        get { return _current; }
        private set
        {
            if (_current != value)
            {
                _current = value;
                OnPropertyChanged("Current");
            }
        }
    }

    /// <summary>
    /// The token associated with the current user which is used to 
    /// do subsequent requests without having to sign in.
    /// </summary>
    public string Token { get; private set; }

    /// <summary>
    /// Indicates wheather a user is signed in or not.
    /// </summary>
    public bool IsSignedIn { get { return Current != null; } }

    /// <summary>
    /// Initiates sign-in based on the username and token stored in user settings.
    /// </summary>
    public void SignInFromLocalStorage(EventHandler<RequestEventArgs> callback)
    {
      string username = null;
      string token = null;

			// see if the browser cookie is present
			//
			string cookie = WebUtil.GetCookie("esri_auth");
			if (cookie != null)
			{
				cookie = HttpUtility.UrlDecode(cookie);
				CachedSignIn signIn = WebUtil.ReadObject<CachedSignIn>(new MemoryStream(Encoding.UTF8.GetBytes(cookie)));
				username = signIn.Email;
				token = signIn.Token;
			}
      
      if (username != null && token != null)
      {
        string url = _agol.Url + "community/users/" + username + "?token=" + token + "&f=json";
        WebUtil.OpenReadAsync(url, null, (sender, e) =>
          {
            if (e.Error != null)  // bail on error
            {
              if (callback != null)
                callback(null, new RequestEventArgs() { Error = e.Error });
              return;
            }

            User cachedUser = WebUtil.ReadObject<User>(e.Result);

            if (cachedUser != null && !string.IsNullOrEmpty(cachedUser.Username))
            {
                // Add credential to IdentityManager
                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username)
                && IdentityManager.Current != null
                && IdentityManager.Current.FindCredential(_agol.Url, username) == null)
                {
                    IdentityManager.Credential cred = new IdentityManager.Credential()
                    {
                        UserName = username,
                        Url = _agol.Url,
                        Token = token
                    };
                    IdentityManager.Current.AddCredential(cred);
                }

                Current = cachedUser;
                Token = token;
            }
            else
            {
                Current = null;
                Token = null;
            }

            if (callback != null)  // notify caller of success
              callback(null, new RequestEventArgs());

            if (SignedInOut != null)
              SignedInOut(null, EventArgs.Empty);
          });
      }
      else if(callback != null)
        callback(null, new RequestEventArgs()); // call the client back in any case
    }

    /// <summary>
    /// Initializes the current user instance
    /// </summary>
    public Task InitializeTaskAsync(string user, string token)
    {
        var tcs = new TaskCompletionSource<bool>();

        GetUser(user, token, (object sender2, GetUserEventArgs e) =>
        {
            if (e.Error == null)
            {
                Current = e.User;
                Token = token;
            }
            else
            {
                Current = null;
                Token = null;
                tcs.TrySetException(e.Error);
            }

            SaveTokenToStorage();

            // raise event
            if (SignedInOut != null)
                SignedInOut(null, EventArgs.Empty);

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    /// <summary>
    /// Signs out of the current AGOL session.
    /// </summary>
    public void SignOut()
    {
      if (Current == null)
        return;

      string agolDomain = _agol.Url.ToLower().Replace("http://", "").Replace("https://", "");
        // See if any credentials for the current user have been placed in the global store
        IEnumerable<IdentityManager.Credential> credentials = CredentialManagement.Current.Credentials.Where(
            c => c.UserName == Current.Username && (c.Url.ToLower().Contains(agolDomain)));
        foreach (IdentityManager.Credential cred in credentials.ToArray())
            CredentialManagement.Current.Credentials.Remove(cred);

      Current = null;
      Token = null;
      DeleteTokenFromStorage();

      if (SignedInOut != null)
        SignedInOut(this, EventArgs.Empty);
    }

    /// <summary>
    /// Updates the current user information on the client.
    /// </summary>
    /// <remarks>
    /// This method is used to make sure the information about the current user on the client are up to date.
    /// </remarks>
    public void RefreshCurrent(EventHandler<RequestEventArgs> callback)
    {
      if (Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      GetUser(Current.Username, Token, (object sender, GetUserEventArgs e) =>
      {
        Current = e.User;

        if(callback != null)
          callback(this, new RequestEventArgs() { Error = e.Error });
      });
    }

    public void GetPublicOrgGroups(EventHandler<GroupsEventArgs> callback)
    {
        getGroups(callback, true);
    }

    public void GetMyOrgGroups(EventHandler<GroupsEventArgs> callback)
    {
        getGroups(callback, false);
    }

    private void getGroups(EventHandler<GroupsEventArgs> callback, bool publicOnly)
    {
        if (Current == null)
        {
            if (callback != null)
                callback(null, new GroupsEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
            return;
        }

        string accountId = Current.AccountId;

        string url = null;
        if (publicOnly)
            url = _agol.Url + "community/groups?q=(accountid:" + accountId + "%20AND%20(access:public))&sortField=title&sortOrder=asc&start=1&num=100&f=json&token=" + Token;
        else
            url = _agol.Url + "community/groups?q=(accountid:" + accountId + "%20AND%20(access:account%20||%20access:public))&sortField=title&sortOrder=asc&start=1&num=100&f=json&token=" + Token;

        // add a bogus parameter to avoid the caching that happens with the WebClient
        //
        url += "&tickCount=" + Environment.TickCount.ToString();

        WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
            if (e.Error != null)
            {
                if (callback != null)
                    callback(this, new GroupsEventArgs() { Error = e.Error });
                return;
            }

            GroupResults result = WebUtil.ReadObject<GroupResults>(e.Result);

            if (callback != null && result != null && result.Groups != null)
                callback(null, new GroupsEventArgs() { Groups = result.Groups });
            else if (callback != null)
                callback(null, new GroupsEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToRetrieveOrgGroups) });
        });
    }

    /// <summary>
    /// Gets the tags for the current user.
    /// </summary>
    public void GetTags(EventHandler<UserTagEventArgs> callback)
    {
      if (Current == null)
      {
        if (callback != null)
			callback(null, new UserTagEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      string userName = Current.Username;
      string url = _agol.Url + "community/users/" + userName + "/tags?f=json&token=" + Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
      {
        if (e.Error != null)
        {
          if (callback != null)
            callback(this, new UserTagEventArgs() { Error = e.Error });
          return;
        }

        Tags result = WebUtil.ReadObject<Tags>(e.Result);

        if (callback != null && result != null && result.UserTags != null)
          callback(null, new UserTagEventArgs() { UserTags = result.UserTags });
        else if (callback != null)
          callback(null, new UserTagEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToRetrieveTagForUser, userName)) });
      });
    }

    /// <summary>
    /// Gets information about the specified user.
    /// </summary>
    private void GetUser(string username, string token, EventHandler<GetUserEventArgs> callback)
    {
        string url = _agol.Url + "community/users/" + username + "?f=json";        
        if (!string.IsNullOrEmpty(token))
            url += "&token=" + token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, null, (sender, e) =>
        {
          if(e.Error != null)
          {
            if (callback != null)
              callback(this, new GetUserEventArgs() { Error = e.Error });
            return;
          }

          User user = WebUtil.ReadObject<User>(e.Result);

          //setup the group thumbnails
          //
          if (user.Groups != null)
            foreach (Group group in user.Groups)
              if (group.ThumbnailPath != null)
              {
                string thumbnailUrl = _agol.Url + "community/groups/" + group.Id + "/info/" + group.ThumbnailPath;
                if (!string.IsNullOrEmpty(token))
                    thumbnailUrl += "?token=" + token;
                group.Thumbnail = new BitmapImage(new Uri(thumbnailUrl));
              }

          if (callback != null)
            callback(null, new GetUserEventArgs() { User = user });
        });
    }

    ///// <summary>
    ///// Gets the group invitations for the current user.
    ///// </summary>
    //public void GetGroupInvitations(EventHandler<GroupInvitationEventArgs> callback)
    //{
    //  if (Current == null)
    //  {
    //    if (callback != null)
    //      callback(null, new GroupInvitationEventArgs() { Error = new Exception("No user signed in.") });
    //    return;
    //  }

    //  string url = _agol.Url + "community/users/" + Current.Username + "/invitations?f=json&token=" + Token;

    //  // add a bogus parameter to avoid the caching that happens with the WebClient
    //  //
    //  url += "&tickCount=" + Environment.TickCount.ToString();

    //  WebUtil.OpenReadAsync(url, null, (object sender, OpenReadCompletedEventArgs e) =>
    //  {
    //    if (e.Error != null)
    //    {
    //      if (callback != null)
    //        callback(null, new GroupInvitationEventArgs() { Error = e.Error });
    //      return;
    //    }

    //    GroupInvitations result = WebUtil.ReadObject<GroupInvitations>(e.Result);

    //    if (callback != null && result != null && result.Invitations != null)
    //      callback(null, new GroupInvitationEventArgs() { Invitations = result.Invitations });
    //    else if(callback != null)
    //      callback(null, new GroupInvitationEventArgs() { Error = new Exception("Failed to get group invitations for " + Current.Username) });
    //  });
    //}

    /// <summary>
    /// Accept the invitation to join a group.
    /// </summary>
    /// <param name="groupId">The group to join.</param>
    /// <param name="callback"></param>
    public void AcceptInvitation(string groupId, EventHandler<RequestEventArgs> callback)
    {
      if (Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/users/" + Current.Username + "/invitations/" + groupId + "/accept?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToAcceptGroupInvitationForGroup, groupId, Current.Username)) });
        });
    }

    /// <summary>
    /// Decline the invitation to join a group.
    /// </summary>
    /// <param name="groupId">The group to join.</param>
    /// <param name="callback"></param>
    public void DeclineInvitation(string groupId, EventHandler<RequestEventArgs> callback)
    {
      if (Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/users/" + Current.Username + "/invitations/" + groupId + "/decline?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
			  callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToDeclineGroupInvitationForGroup, groupId, Current.Username)) });
        });
    }

    /// <summary>
    /// Deletes the specified notification.
    /// </summary>
    public void DeleteNotification(string notificationId, EventHandler<RequestEventArgs> callback)
    {
      if (Current == null)
      {
        if (callback != null)
			callback(null, new RequestEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionNoUserSignedIn) });
        return;
      }

      List<HttpContentItem> items = new List<HttpContentItem>();
      items.Add(new HttpContentItem() { Name = "f", Value = "json" });

      string url = _agol.Url + "community/users/" + Current.Username + "/notifications/" + notificationId + "/delete?token=" + _agol.User.Token;

      WebUtil.MultiPartPostAsync(url, items, ApplicationUtility.Dispatcher, (sender, e) =>
        {
          Success result = WebUtil.ReadObject<Success>(e.Result);

          if (result != null && result.Succeeded && callback != null)
            callback(null, new RequestEventArgs() { });
          else if (callback != null)
            callback(null, new RequestEventArgs() { Error = new Exception(string.Format(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.ExceptionFailedToDeleteNoitification, notificationId, Current.Username)) });
        });
    }

    /// <summary>
    /// Performs a search for users using the specified query.
    /// </summary>
    public void Search(string query, EventHandler<UserSearchEventArgs> callback)
    {
      string url = _agol.Url + "community/users?num=100&q=" + query + "&f=json";
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
    /// Performs a search for users using the specified query and startIndex.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="startIndex">The index of the starting result for paging.</param>
    /// <param name="maxCount"></param>
    /// <param name="callback"></param>
    public void Search(string query, int startIndex, int maxCount, EventHandler<UserSearchEventArgs> callback)
    {
      string url = _agol.Url + "community/users?start=startIndex&num=maxCount&q=" + query + "&f=json";
      if (_agol.User.Current != null)
        url += "&token=" + _agol.User.Token;

      // add a bogus parameter to avoid the caching that happens with the WebClient
      //
      url += "&tickCount=" + Environment.TickCount.ToString();

      WebUtil.OpenReadAsync(url, callback, (sender, e) =>
      {
        SearchCompleted(e, callback);
      });
    }

    /// <summary>
    /// Called when a search for users completes.
    /// </summary>
    void SearchCompleted(OpenReadEventArgs e, EventHandler<UserSearchEventArgs>  callback)
    {
      if (e.Error != null)
      {
        if (callback != null)
          callback(this, new UserSearchEventArgs() { Error = e.Error });
        return;
      }

      UserSearchResult result = WebUtil.ReadObject<UserSearchResult>(e.Result);

      if (callback != null && result != null)
        callback(null, new UserSearchEventArgs() { Result = result });
      else if(callback != null)
        callback(null, new UserSearchEventArgs() { Error = new Exception(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.Resources.Strings.UserSearchFailed) });
    }

    /// <summary>
    /// Saves the current sign-in information (username+token) as a browser cookie
    /// and to local storage so automatic sign-in can occur with the next session. If the user is not
    /// signed in, removes the token from local storage - keeps the username to 
    /// seed the sign-in dialog.
    /// </summary>
    void SaveTokenToStorage()
    {
      if (Current != null) // success
      {
        // save the cookie
        //
        CachedSignIn signIn = new CachedSignIn() { Email = Current.Username, FullName = Current.FullName, Token = this.Token };
        string json = WebUtil.SaveObject<CachedSignIn>(signIn);
        WebUtil.SetCookie("esri_auth", HttpUtility.UrlEncode(json), ArcGISOnlineEnvironment.HostDomain, "/", 14);
      }
      else
        WebUtil.DeleteCookie("esri_auth", ArcGISOnlineEnvironment.HostDomain);

			// save to username to isolated storage to seed the sign-in dialog later
			//
			if (!IsolatedStorageFile.IsEnabled || Current == null)
				return;

			IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

			if (settings.Contains("username"))
				settings["username"] = Current.Username;
			else
				settings.Add("username", Current.Username);

      settings.Save();
    }

    internal void DeleteTokenFromStorage()
    {
        WebUtil.DeleteCookie("esri_auth", ArcGISOnlineEnvironment.HostDomain);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }

}
