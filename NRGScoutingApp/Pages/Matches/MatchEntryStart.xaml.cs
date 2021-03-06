﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {
    public partial class MatchEntryStart : ContentPage {
        private Enum goToMatch;

        public MatchEntryStart(Enum ismatch) {
            goToMatch = ismatch;
            InitializeComponent();
            //MatchesList.ItemsSource = teams;
            MatchesList.RefreshCommand = new Command(() =>
            {
                //Do your stuff.
                MatchesList.IsRefreshing = true;
                
                try
                {
                    DataDownload.refreshTeams();
                }
                catch (Exception ex)
                {
                    DisplayAlert(ex.ToString(), "", "OK");
                }
                populateTeamList(MatchesList);
                MatchesList.IsRefreshing = false;
            });
            populateTeamList(MatchesList);
        }

        List<string> teams = new List<string>();

        async void populateTeamList(ListView listView)
        {
            Debug.WriteLine("start" + App.teamsList.Count);
            await Task.Run(() =>
            {
                if (App.teamsList.Count <= 0)
                {
                    Debug.WriteLine("Whatt");
                    DataDownload.populateTeamList(Preferences.Get(ConstantVars.TEAM_LIST_STORAGE, "[]"), App.teamsList);
                }
                setSimplifiedTeams(App.teamsList, teams);
            });
            Debug.WriteLine("wre" + App.teamsList.Count);
                listView.ItemsSource = null;
                listView.ItemsSource = teams;
        }

        public Boolean goBack = false;
        public string teamName;

        protected override void OnAppearing() {
            if (goBack == true) {
                goBack = false;
                Navigation.PopAsync();
            }
            MatchesList.ItemsSource = teams;
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e) {
            teamName = e.Item.ToString();
            int teamnum;
            try
            {
                teamnum = AdapterMethods.getTeamInt(teamName, App.teamsList);
                Preferences.Set("teamStart", teamnum);
                switch (goToMatch)
                {
                    case ConstantVars.TEAM_SELECTION_TYPES.match:
                        await Navigation.PushAsync(new MatchEntryEditTab() { Title = teamName });
                        Navigation.RemovePage(this);
                        break;
                    case ConstantVars.TEAM_SELECTION_TYPES.pit:
                        await Navigation.PushAsync(new PitEntry(true, teamnum, true) { Title = teamName });
                        Navigation.RemovePage(this);
                        break;
                    case ConstantVars.TEAM_SELECTION_TYPES.teamSelection:
                        await Navigation.PopAsync();
                        break;

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Failed to get team number from the list", "", "OK");
                System.Diagnostics.Debug.WriteLine(ex);
            }

        }

        void setSimplifiedTeams(Dictionary<int, string> dict, List<string> s)
        { 
            foreach (KeyValuePair<int, string> pair in dict)
            {
                s.Add(pair.Key + " - " + pair.Value);
            }
        }


        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e) {
            // MatchesList.BeginRefresh();
            if (!String.IsNullOrWhiteSpace(e.NewTextValue)) {
                MatchesList.ItemsSource = teams.Where(teams => (teams.ToLower().Contains(e.NewTextValue.ToLower()))); //|| teams.Value.ToString().ToLower().Contains(e.NewTextValue.ToLower())));
            }
            else
            {
                MatchesList.ItemsSource = teams;
            }

            //MatchesList.EndRefresh();
        }
    
    }

}
