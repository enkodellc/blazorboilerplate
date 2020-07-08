﻿using BlazorBoilerplate.Shared.Dto.Db;
using BlazorBoilerplate.Shared.Interfaces;
using BlazorBoilerplate.Localization;
using Humanizer;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BlazorBoilerplate.Shared.Services
{
    public class AppState
    {
        public event Action OnChange;

        private readonly IApiClient _apiClient;
        private UserProfile _userProfile { get; set; }

        private readonly IStringLocalizer<Strings> L;

        public IConfiguration _configuration { get; }

        public readonly string AppName = "";
        public readonly string AppShortName = "";
        public readonly string BreadCrumbHome = "";
        public readonly bool ForceLogin = false;

        public AppState(IApiClient apiClient, IStringLocalizer<Strings> l, IConfiguration configuration)
        {
            L = l;
            _configuration = configuration;
            AppName = L["AppName"].ToString().Humanize(LetterCasing.Title);
            AppShortName = L["AppShortName"].ToString().Humanize(LetterCasing.Title);
            BreadCrumbHome = L["BreadCrumbHome"].ToString().ToUpper();
            ForceLogin = _configuration.GetSection("BlazorBoilerplate:ForceLogin").Get<bool>();
            _apiClient = apiClient;
        }

        public bool IsNavOpen
        {
            get
            {
                if (_userProfile == null)
                    return true;

                return _userProfile.IsNavOpen;
            }
            set
            {
                _userProfile.IsNavOpen = value;
            }
        }
        public bool IsNavMinified { get; set; }

        public async Task UpdateUserProfile()
        {
            await _apiClient.SaveChanges();
        }

        public void ClearUserProfile()
        {
            _userProfile = null;
        }

        public async Task<UserProfile> GetUserProfile()
        {
            if (_userProfile == null)
                _userProfile = await _apiClient.GetUserProfile();

            return _userProfile;
        }

        public async Task UpdateUserProfileCount(int count)
        {
            _userProfile.Count = count;
            await UpdateUserProfile();
            NotifyStateChanged();
        }

        public async Task<int> GetUserProfileCount()
        {
            if (_userProfile == null)
            {
                _userProfile = await GetUserProfile();
                return _userProfile.Count;
            }

            return _userProfile.Count;
        }

        public async Task SaveLastVisitedUri(string uri)
        {
            if (_userProfile == null)
            {
                _userProfile = await GetUserProfile();
            }
            else
            {
                _userProfile.LastPageVisited = uri;
                await UpdateUserProfile();

                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
