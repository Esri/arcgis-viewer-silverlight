/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Mapping.Core;
using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.Serialization;
using System.Windows.Browser;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;
using System.Windows;
using System.IO;
using ESRI.ArcGIS.Client;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Toolkit;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Represents the ArcGIS Online services.
  /// </summary>
  public class ArcGISOnline : INotifyPropertyChanged
  {
      bool m_ignoreSignInOut;

    public ArcGISOnline()
    {
      Group = new AGOLGroup(this);
      Content = new AGOLContent(this);
      User = new AGOLUser(this);

      // re-initialize portal if user signs in or out
      User.SignedInOut += (o, e) =>
        {
            if (IsInitialized && !m_ignoreSignInOut)
                Initialize(Url, UrlSecure);
        };
    }

    public void Initialize(string url, string urlSecure, bool forceBrowserAuth = false, bool signOutCurrentUser = false)
    {
        _isInitialized = false;
        InitializationError = null;

        if (User != null && signOutCurrentUser)
        {
            m_ignoreSignInOut = true;
            User.SignOut();
            removePortalCredentials(Url);
            m_ignoreSignInOut = false;
        }

        var oldUrl = Url;
        var oldUrlSecure = UrlSecure;
        var oldPortalInfo = PortalInfo;
        Url = url;
        UrlSecure = urlSecure;
        PortalInfo = null;

        // Initialize PortalInfo 
        string portalInfoUrl = string.Format("{0}/{1}?f=json&culture={2}", url, "accounts/self", 
            System.Threading.Thread.CurrentThread.CurrentUICulture.ToString());
        if (!string.IsNullOrEmpty(User.Token))
            portalInfoUrl += "&token=" + User.Token;

        WebUtil.OpenReadAsync(portalInfoUrl + "&r=" + DateTime.Now.Ticks, null, (o, e) =>
        {
            if (e.Error == null && e.Result != null)
            {
                PortalInfo = WebUtil.ReadObject<PortalInfo>(e.Result);
                if (PortalInfo != null && PortalInfo.User != null 
                && !string.IsNullOrEmpty(PortalInfo.User.UserName) 
                && string.IsNullOrEmpty(User.Token))
                    initBrowserAuthenticatedUser(portalInfoUrl, PortalInfo.User.UserName);
            }
            else
            {
                Url = oldUrl;
                UrlSecure = oldUrlSecure;
                PortalInfo = oldPortalInfo;
                InitializationError = e.Error;
            }

            // Fire initialized event
            _isInitialized = true;
            OnInitialized();
        }, forceBrowserAuth);
    }

    private void initBrowserAuthenticatedUser(string portalUrl, string userName)
    {
        if (!CredentialManagement.Current.Credentials.Any(c => c.Url != null
        && c.Url.Equals(portalUrl, StringComparison.OrdinalIgnoreCase)
        && c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)))
        {
            IdentityManager.Credential cred = new IdentityManager.Credential()
            {
                Url = portalUrl,
                UserName = userName
            };
            CredentialManagement.Current.Credentials.Add(cred);
        }

        m_ignoreSignInOut = true;
        User.InitializeTaskAsync(userName, null).ContinueWith(t =>
        {
            m_ignoreSignInOut = false;
            if (t.IsFaulted)
            {
                var exception = t.Exception;
            }
            else
            {
                // Do nothing
            }
        });                
    }

    private void removePortalCredentials(string portalUrl)
    {
        if (portalUrl == null)
            return;

        string agolDomain = portalUrl.ToLower().Replace("http://", "").Replace("https://", "");
        // See if any credentials for the current user have been placed in the global store
        IEnumerable<IdentityManager.Credential> credentials = 
            CredentialManagement.Current.Credentials.Where(c => c.Url.ToLower().Contains(agolDomain));
        foreach (IdentityManager.Credential cred in credentials.ToArray())
            CredentialManagement.Current.Credentials.Remove(cred);
    }

    private string _url;
    /// <summary>
    /// The base Url for the service.
    /// </summary>
    public string Url
    {
        get { return _url; }
        private set
        {
            if (_url != value)
            {
                _url = value;
                if (!string.IsNullOrEmpty(_url))
                    OAuthAuthorize.Initialize(_url);
                OnPropertyChanged("Url");
            }
        }
    }

      private string _urlSecure;
    /// <summary>
    /// The secure base Url for the sign-in.
    /// </summary>
      public string UrlSecure
      {
          get { return _urlSecure; }
          private set
          {
              if (_urlSecure != value)
              {
                  _urlSecure = value;
                  OnPropertyChanged("UrlSecure");
              }
          }
      }

    /// <summary>
    /// Provides methods to interact with groups.
    /// </summary>
    public AGOLGroup Group { get; private set; }

    /// <summary>
    /// Provides methods to interact with content.
    /// </summary>
    public AGOLContent Content { get; private set; }

    private AGOLUser _user;
    /// <summary>
    /// Provides methods to interact with users.
    /// </summary>
    public AGOLUser User 
    {
        get { return _user; }
        private set
        {
            if (_user != value)
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }
    }

    private PortalInfo _portalInfo = null;
    /// <summary>
    /// Provides instance-level information about the ArcGIS Portal
    /// </summary>
    public PortalInfo PortalInfo 
    { 
        get { return _portalInfo; }
        private set
        {
            if (_portalInfo != value)
            {
                _portalInfo = value;
                OnPropertyChanged("PortalInfo");
            }
        }
    }

    private bool _isInitialized = false;
    /// <summary>
    /// Gets whether or not the instance is initialized
    /// </summary>
    public bool IsInitialized 
    {
        get { return _isInitialized; }
        private set
        {
            if (_isInitialized != value)
            {
                _isInitialized = value;
                OnPropertyChanged("IsInitialized");
            }
        }
    }

    private Exception _initializationError;
    /// <summary>
    /// Gets the error that occurred during intialization.  Will only be populated if the last attempt to intialize
    /// failed.
    /// </summary>
    public Exception InitializationError
    {
        get { return _initializationError; }
        private set
        {
            if (_initializationError != value)
            {
                _initializationError = value;
                OnPropertyChanged("InitializationError");
            }
        }
    }

    /// <summary>
    /// Fires when the instance is initialized
    /// </summary>
    public event EventHandler<EventArgs> Initialized;

    private void OnInitialized()
    {
        if (Initialized != null)
            Initialized(this, EventArgs.Empty);
    }
  
    public event PropertyChangedEventHandler  PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  #region EventArgs

  /// <summary>
  /// Represents the base class for all event args used in a AGOL rest request callback.
  /// Provides error information.
  /// </summary>
  public class RequestEventArgs : EventArgs
  {
    public Exception Error { get; set; }
  }

  /// <summary>
  /// Provides response for a request of a user.
  /// </summary>
  public class GetUserEventArgs : RequestEventArgs
  {
    public User User { get; set; }
  }

  /// <summary>
  /// Provides response for a group search.
  /// </summary>
  public class GroupSearchEventArgs : RequestEventArgs
  {
    public GroupSearchResult Result { get; set; }
  }

  /// <summary>
  /// Provides response for a user search.
  /// </summary>
  public class UserSearchEventArgs : RequestEventArgs
  {
    public UserSearchResult Result { get; set; }
  }


  /// <summary>
  /// Provides response for a request for members of a group.
  /// </summary>
  public class GroupMembersEventArgs : RequestEventArgs
  {
    public GroupMembers GroupMembers { get; set; }
  }

  /// <summary>
  /// Provides response for a request that returns a collection of ContentItems.
  /// </summary>
  public class ContentItemsEventArgs : RequestEventArgs
  {
    public ContentItem[] Items { get; set; }
  }

  /// <summary>
  /// Provides response for a request related to a Group.
  /// </summary>
  public class GroupEventArgs : RequestEventArgs
  {
    public Group Group { get; set; }
  }

  /// <summary>
  /// Provides response for a request related to a Group.
  /// </summary>
  public class GroupsEventArgs : RequestEventArgs
  {
      public Group[] Groups { get; set; }
  }

    /// <summary>
  /// Provides response for a content search.
  /// </summary>
  public class ContentSearchEventArgs : RequestEventArgs
  {
    public ContentSearchResult Result { get; set; }
  }

  /// <summary>
  /// Provides response for a request for sharing information of a content item.
  /// </summary>
  public class SharingInfoEventArgs : ContentItemEventArgs
  {
    public SharingInfo SharingInfo { get; set; }
  }

  /// <summary>
  /// Provides data for delegates that operate on ContentItems.
  /// </summary>
  public class ContentItemEventArgs : RequestEventArgs
  {
    public ContentItem Item { get; set; }
    public string Id { get; set; } //populated in case of an error
  }

  /// <summary>
  /// Provides response for a request of comments for a content item.
  /// </summary>
  public class CommentEventArgs : RequestEventArgs
  {
    public Comment[] Comments { get; set; }
  }

  /// <summary>
  /// Provides response for a request of notifications for a user.
  /// </summary>
  public class UserTagEventArgs : RequestEventArgs
  {
    public UserTag[] UserTags { get; set; }
  }

  /// <summary>
  /// Provides response for a request of applications to join a group.
  /// </summary>
  public class GroupApplicationEventArgs : RequestEventArgs
  {
    public GroupApplication[] Applications { get; set; }
  }

  /// <summary>
  /// Provides response for a request of group invitations for a user.
  /// </summary>
  public class GroupInvitationEventArgs : RequestEventArgs
  {
    public GroupInvitation[] Invitations { get; set; }
  }

	/// <summary>
	/// Provides response for a request for a user's content.
	/// </summary>
	public class UserContentEventArgs : RequestEventArgs
	{
		public UserContent Content { get; set; }
	}
	
	#endregion

  #region Json objects

  /// <summary>
  /// Represents the ArcGIS Online item.
  /// </summary>
  [DataContract]
  public class ContentItem : INotifyPropertyChanged
  {
    ImageSource _thumbnail;
    string _data;
		ContentFolder _folder;

    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "item")]
    public string Item { get; set; }

    [DataMember(Name = "itemType")]
    public string ItemType { get; set; }

    [DataMember(Name = "owner")]
    public string Owner { get; set; }

    [DataMember(Name = "uploaded")]
    public Int64 Uploaded { get; set; }

    [DataMember(Name = "modified")]
    public Int64 Modified { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    [DataMember(Name = "type")]
    public string Type { get; set; }

    [DataMember(Name = "typeKeywords")]
    public string[] TypeKeywords { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }

    [DataMember(Name = "tags")]
    public string[] Tags { get; set; }

    public string TagList
    {
      get
      {
        string tags = "";
				if (Tags != null)
					foreach (string tag in Tags)
					{
						if (tags.Length > 0)
							tags += ", ";
						tags += tag;
					}

        return tags;
      }
    }

    [DataMember(Name = "thumbnail")]
    public string ThumbnailPath { get; set; }

    [DataMember(Name = "access")]
    public string Access { get; set; }

    [DataMember(Name = "accessInformation")]
    public string AccessInformation { get; set; }

    [DataMember(Name = "snippet")]
    public string Summary { get; set; }

    //[DataMember(Name = "spatialReference")]
    //public int SpatialReference { get; set; }

    [DataMember(Name = "extent")]
    public double[][] Extent { get; set; }

		int _numRatings = 0;
		[DataMember(Name = "numRatings")]
		public int NumberOfRatings
		{
			get { return _numRatings; }
			set
			{
				if (value < 0) //temp fix for negative values being returned from the server occassionally
					_numRatings = 0;
				else
					_numRatings = value;
			}
		}

		double _avgRating = 0;
		[DataMember(Name = "avgRating")]
		public double AverageRating
		{
			get { return _avgRating; }
			set
			{
				if (value < 0) //temp fix for negative values being returned from the server occassionally
					_avgRating = 0;
				else
					_avgRating = value;
			}
		}

    public ImageSource Thumbnail
    {
      get { return _thumbnail; }
      set
      {
        _thumbnail = value;
        NotifyPropertyChanged("Thumbnail");
      }
    }

    /// <summary>
    /// Get/sets the actual data for this item. To retrieve it, use ContentService.GetItemDataAsync.
    /// </summary>
    public string Data
    {
      get { return _data; }
      set
      {
        _data = value;
        NotifyPropertyChanged("Data");
      }
    }

		public ContentFolder Folder
		{
			get { return _folder; }
			set
			{
				_folder = value;
				NotifyPropertyChanged("Folder");
			}
		}

    void NotifyPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }

  /// <summary>
  /// Represents the base class for results of a search.
  /// </summary>
  [DataContract]
  public class SearchResult
  {
    [DataMember(Name = "query")]
    public string Query { get; set; }

    [DataMember(Name = "total")]
    public int TotalCount { get; set; }

    [DataMember(Name = "start")]
    public int StartIndex { get; set; }

    [DataMember(Name = "num")]
    public int Count { get; set; }

    [DataMember(Name = "nextStart")]
    public int NextStart { get; set; }
  }

  /// <summary>
  /// Represents the results of a content search.
  /// </summary>
  [DataContract]
  public class ContentSearchResult : SearchResult
  {
    public string Sort { get; set; }

    [DataMember(Name = "results")]
    public ContentItem[] Items { get; set; }
  }

  /// <summary>
  /// Represents the results of a group search.
  /// </summary>
  [DataContract]
  public class GroupSearchResult : SearchResult
  {
    [DataMember(Name = "results")]
    public Group[] Items { get; set; }
  }

  /// <summary>
  /// Represents the results of a user search.
  /// </summary>
  [DataContract]
  public class UserSearchResult : SearchResult
  {
    [DataMember(Name = "results")]
    public User[] Items { get; set; }
  }

  /// <summary>
  /// Represents a session with AGOL - a token that can be used for queries that
  /// take credentials and the current user.
  /// </summary>
  [DataContract]
  public class SignIn
  {
    [DataMember(Name = "token")]
    public string Token { get; set; }

    public User User { get; set; }
  }

  /// <summary>
  /// Represents the sign in information that can be cached as a browser cookie.
  /// </summary>
  [DataContract]
  public class CachedSignIn
  {
    [DataMember(Name = "email")]
    public string Email { get; set; }

    [DataMember(Name = "fullName")]
    public string FullName { get; set; }

    [DataMember(Name = "token")]
    public string Token { get; set; }
  }

  /// <summary>
  /// Represents the members of a group.
  /// </summary>
  [DataContract]
  public class GroupMembers
  {
    [DataMember(Name = "owner")]
    public string Owner { get; set; }

    [DataMember(Name = "admins")]
    public string[] Admins { get; set; }

    [DataMember(Name = "users")]
    public string[] Users { get; set; }
  }

  /// <summary>
  /// Represents the response of a request that reports only if the request has succeeded or not.
  /// </summary>
  [DataContract]
  public class Success
  {
    [DataMember(Name = "success")]
    public bool Succeeded { get; set; }
  }

  /// <summary>
  /// Represents the response of a request to share an item.
  /// </summary>
  [DataContract]
  public class ShareItemResult
  {
    [DataMember(Name = "notSharedWith")]
    public string[] NotSharedWith { get; set; }

    [DataMember(Name = "itemId")]
    public string ItemId { get; set; }
  }

  /// <summary>
  /// Represents the response of a request for related items.
  /// </summary>
  [DataContract]
  public class RelatedItemsResult
  {
    [DataMember(Name = "relatedItems")]
    public ContentItem[] RelatedItems { get; set; }
  }

  /// <summary>
  /// Represents the response of a request for sharing information of a content item.
  /// </summary>
  [DataContract]
  public class UserItem
  {
    [DataMember(Name = "item")]
    public ContentItem ContentItem { get; set; }

    [DataMember(Name = "sharing")]
    public SharingInfo Sharing { get; set; }
  }

	/// <summary>
	/// Represents the content of a user.
	/// </summary>
	[DataContract]
	public class UserContent
	{
		[DataMember(Name = "items")]
		public ContentItem[] ContentItems { get; set; }
		[DataMember(Name = "folders")]
		public ContentFolder[] Folders { get; set; }
	}

	/// <summary>
	/// Represents a folder in the user's content.
	/// </summary>
	[DataContract]
	public class ContentFolder
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }

		public static bool IsSubfolder(ContentFolder folder)
		{
			return (folder != null && !string.IsNullOrEmpty(folder.Id));
		}
	}
	
	/// <summary>
  /// Represents the sharing information of a ContentItem.
  /// </summary>
  [DataContract]
  public class SharingInfo
  {
    [DataMember(Name = "access")]
    public string Access { get; set; }

    [DataMember(Name = "groups")]
    public string[] GroupIds { get; set; }
  }

  /// <summary>
  /// Represents an AGOL user.
  /// </summary>
  [DataContract]
  public class User
  {
    [DataMember(Name = "username")]
    public string Username { get; set; }

    [DataMember(Name = "fullName")]
    public string FullName { get; set; }

    [DataMember(Name = "email")]
    public string Email { get; set; }

    [DataMember(Name = "groups")]
    public Group[] Groups { get; set; }

    [DataMember(Name = "accountId", IsRequired = false)]
    public string AccountId { get; set; }
  }

  /// <summary>
  /// Represents an AGOL group.
  /// </summary>
  [DataContract]
  public class Group
  {
    ImageSource _thumbnail;

    [DataMember(Name = "id", IsRequired = false)]
    public string Id { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    [DataMember(Name = "owner")]
    public string Owner { get; set; }

    [DataMember(Name = "description", IsRequired = false)]
    public string Description { get; set; }

    [DataMember(Name = "thumbnail", IsRequired = false)]
    public string ThumbnailPath { get; set; }

    [DataMember(Name = "created", IsRequired = false)]
    public string Date { get; set; }

    [DataMember(Name = "snippet", IsRequired = false)]
    public string Summary { get; set; }

    [DataMember(Name = "featuredItemsId", IsRequired = false)]
    public string FeaturedItemsId { get; set; }

    [DataMember(Name = "isPublic", IsRequired = false)]
    public bool IsPublic { get; set; }

    [DataMember(Name = "isInvitationOnly", IsRequired = false)]
    public bool IsInvitationOnly { get; set; }

    [DataMember(Name = "tags", IsRequired = false)]
    public string[] Tags { get; set; }

    public ImageSource Thumbnail
    {
      get
      {
        if (_thumbnail != null)
          return _thumbnail;

        return new BitmapImage(new Uri("/ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;component/Images/group64.png", UriKind.Relative));
      }
      set
      {
        _thumbnail = value;
      }
    }

    public string TagList
    {
      get
      {
        string tags = "";
        foreach (string tag in Tags)
        {
          if (tags.Length > 0)
            tags += ", ";
          tags += tag;
        }

        return tags;
      }
    }
  }


  /// <summary>
  /// Represents a collection of groups.
  /// </summary>
  [DataContract]
  public class GroupResults
  {
      [DataMember(Name = "results", IsRequired = false)]
      public Group[] Groups { get; set; }

      //[DataMember(Name = "total", IsRequired = false)]
      //public int Total { get; set; }

      //[DataMember(Name = "start", IsRequired = false)]
      //public int Start { get; set; }

      //[DataMember(Name = "nextStart", IsRequired = false)]
      //public int NextStart { get; set; }

      //[DataMember(Name = "num", IsRequired = false)]
      //public int Number { get; set; }

  }

  /// <summary>
  /// Represents the response of a request to unshare an item.
  /// </summary>
  [DataContract]
  public class UnshareItemResult
  {
    [DataMember(Name = "notUnsharedFrom")]
    public string[] NotUnsharedFrom { get; set; }

    [DataMember(Name = "itemId")]
    public string ItemId { get; set; }
  }

  /// <summary>
  /// Represents a collection of comments for a content item.
  /// </summary>
  [DataContract]
  public class ItemComments
  {
    [DataMember(Name = "comments")]
    public Comment[] Comments { get; set; }
  }

  /// <summary>
  /// Represents the comment for a content item.
  /// </summary>
  [DataContract]
  public class Comment
  {
    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "owner")]
    public string Owner { get; set; }

    [DataMember(Name = "created")]
    public string Created { get; set; }

    [DataMember(Name = "comment")]
    public string Text { get; set; }
  }

  /// <summary>
  /// Represents a collection of user tags.
  /// </summary>
  [DataContract]
  public class Tags
  {
    [DataMember(Name = "tags")]
    public UserTag[] UserTags { get; set; }
  }

  /// <summary>
  /// Represents the a users tag.
  /// </summary>
  [DataContract]
  public class UserTag
  {
    [DataMember(Name = "tag")]
    public string Tag { get; set; }

    [DataMember(Name = "count")]
    public int Count { get; set; }
  }

  /// <summary>
  /// Represents a collection of applications to join a group.
  /// </summary>
  [DataContract]
  public class GroupApplications
  {
    [DataMember(Name = "applications")]
    public GroupApplication[] Applications { get; set; }
  }

  /// <summary>
  /// Represents a application to join a group.
  /// </summary>
  [DataContract]
  public class GroupApplication
  {
    [DataMember(Name = "username")]
    public string Username { get; set; }
    [DataMember(Name = "received")]
    public string Received { get; set; }
  }

  /// <summary>
  /// Represents a collection of invitations for a user to join groups.
  /// </summary>
  [DataContract]
  public class GroupInvitations
  {
    [DataMember(Name = "userInvitations")]
    public GroupInvitation[] Invitations { get; set; }
  }

  /// <summary>
  /// Represents a invitation for a user to join a group.
  /// </summary>
  [DataContract]
  public class GroupInvitation
  {
    [DataMember(Name = "username")]
    public string Username { get; set; }
    [DataMember(Name = "received")]
    public string Received { get; set; }
    [DataMember(Name = "fromUsername")]
    public string FromUsername { get; set; }
    [DataMember(Name = "group")]
    public Group Group { get; set; }
  }

  #endregion
}
