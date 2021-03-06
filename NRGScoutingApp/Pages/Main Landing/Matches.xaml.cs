﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp {
    public partial class Matches : ContentPage {
        /* For Blue Alliance Matches
        void Handle_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new BlueAllianceMatches());
        } */

        public static Boolean appRestore;
        public Boolean popNav;
        public static List<MatchesListFormat> matchesList;

        public Matches () {
            InitializeComponent ();
            matchConfirm ();
            Preferences.Set ("newAppear", 0);
            populateMatchesList ();
        }

        protected override void OnAppearing () {
            popNav = false;
            string comp = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME,"Error in name");
            try
            {
                comp = App.eventsList[comp];
            }
            catch { }
            currComp.Text = comp;
            if (!Preferences.ContainsKey (ConstantVars.APP_DATA_STORAGE)) {
                Preferences.Set (ConstantVars.APP_DATA_STORAGE, "");
                Preferences.Set ("tempMatchEvents", "");
            }
            if (!Preferences.ContainsKey ("newAppear")) { } //DEBUG PURPOSES
            else if (Preferences.Get ("newAppear", 0) == 1) {
                Preferences.Set ("appState", 1);
                Preferences.Set ("timerValue", 0);
                Preferences.Set ("newAppear", 0);
                Preferences.Set ("tempMatchEvents", "");
                populateMatchesList ();
            } else if (Preferences.Get ("newAppear", 0) == 0) {
                Preferences.Set ("appState", 0);
                Preferences.Set ("timerValue", 0);
                //Preferences.Set ("teamStart", "");
                Preferences.Set ("newAppear", 0);
                Preferences.Set ("tempMatchEvents", "");
            }
            populateMatchesList ();
        }

        void importClicked (object sender, System.EventArgs e) {
            popupInit ();
        }
        private void popupInit () {
            var popup = new ImportDialog ();
            popup.Disappearing += (sender, e) => { this.OnAppearing (); };
            PopupNavigation.Instance.PushAsync (popup);
        }

        void exportClicked (object sender, System.EventArgs e) {
            PopupNavigation.Instance.PushAsync (new ExportDialog ());
        }

        async void newClicked (object sender, System.EventArgs e) {
            if (String.IsNullOrEmpty(Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "")))
            {
                await Navigation.PushAsync(new ChangeEventPage());
            }
            else
            {
                popNav = false;
                appRestore = false;
                await Navigation.PushAsync(new MatchEntryStart(ConstantVars.TEAM_SELECTION_TYPES.match));
            }
        }

        private void SearchBar_OnTextChanged (object sender, TextChangedEventArgs e) {
            if (string.IsNullOrWhiteSpace (e.NewTextValue)) {
                listView.ItemsSource = matchesList;
            } else {
                listView.ItemsSource = matchesList.Where (matchesList => matchesList.teamNameAndSide.ToLower ().Contains (e.NewTextValue.ToLower ()) || matchesList.matchNum.ToLower ().Contains (e.NewTextValue.ToLower ()));
            }
        }

        void matchConfirm () {
            if (!Preferences.ContainsKey ("appState")) {
                appRestore = false;
                Preferences.Set ("appState", 0);
                Preferences.Set ("teamStart", 0);
                Preferences.Set ("timerValue", (int) 0);
                Preferences.Set ("tempParams", "");
                Preferences.Set ("tempMatchEvents", "");
                Preferences.Set ("tempPitNotes", "");
                Preferences.Set(ConstantVars.CURRENT_EVENT_NAME, "");
            } else if (!String.IsNullOrWhiteSpace (Preferences.Get ("tempMatchEvents", "")) || !String.IsNullOrWhiteSpace (Preferences.Get ("tempParams", ""))) //App.Current.Properties["appState"].ToString() == "1"
            {
                appRestore = true;
                NavigationPage.SetHasNavigationBar (this, false);
                Navigation.PushAsync (new MatchEntryEditTab () { Title = AdapterMethods.getTeamString(Preferences.Get ("teamStart", 0)) });
            } else if (!String.IsNullOrWhiteSpace (Preferences.Get ("tempPitNotes", ""))) {
                appRestore = true;
                NavigationPage.SetHasNavigationBar (this, false);
                Navigation.PushAsync (new PitEntry (true, Preferences.Get ("teamStart", 0), true) { Title = AdapterMethods.getTeamString(Preferences.Get("teamStart", 0)) });
            } else if (Preferences.Get ("appState", 0) == 0) {
                appRestore = false;
                Preferences.Set ("appState", 0);
                Preferences.Set ("timerValue", (int) 0);
                Preferences.Set ("teamStart", 0);
                Preferences.Set ("tempMatchEvents", "");
                Preferences.Set ("tempParams", "");
                Preferences.Set ("tempPitNotes", "");
            }
            if (!Preferences.ContainsKey (ConstantVars.APP_DATA_STORAGE)) {
                Preferences.Set (ConstantVars.APP_DATA_STORAGE, "");
                Preferences.Set ("tempMatchEvents", "");
                Preferences.Set ("tempPitNotes", "");
            }
            populateMatchesList ();
        }

        void Handle_ItemTapped (object sender, Xamarin.Forms.ItemTappedEventArgs e) {
            int index;
            var x = listView.ItemsSource as List<MatchesListFormat>;
            if (!String.IsNullOrWhiteSpace (searchBar.Text)) {
                index = matchesList.IndexOf (e.Item as MatchesListFormat);
            } else {
                index = (listView.ItemsSource as List<MatchesListFormat>).IndexOf (e.Item as MatchesListFormat);
            }
            Navigation.PushAsync (new MatchesDetailView (index));
        }

        public class MatchesListFormat {
            public String matchNum { get; set; }
            public String teamNameAndSide { get; set; }
        }

        async void deleteClicked (object sender, System.EventArgs e) {
            await DisplayAlert ("Hold it", "Make sure export to data first", "OK");
            var del = await DisplayAlert ("Notice", "Do you want to delete all matches? Data CANNOT be recovered.", "Yes", "No");
            if (del) {
                JObject s = JObject.Parse(Preferences.Get (ConstantVars.APP_DATA_STORAGE, "[]"));
                string eventName = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
                if (s.ContainsKey(eventName) && s[eventName].ToObject<JObject>().ContainsKey("Matches"))
                {
                    JObject temp = (JObject)s[eventName];
                    temp.Remove("Matches");
                }
                Preferences.Set(ConstantVars.APP_DATA_STORAGE, JsonConvert.SerializeObject(s));
                populateMatchesList ();
            }
        }

        async void deleteAllClicked(object sender, System.EventArgs e)
        {
            await DisplayAlert("Hold it", "Make sure export to data first", "OK");
            var del = await DisplayAlert("Notice", "Do you want to delete all app data (downloaded teams, etc)? Data CANNOT be recovered.", "Yes", "No");
            if (del)
            {
                Preferences.Set(ConstantVars.APP_DATA_STORAGE, "{}");
                Preferences.Clear();
                populateMatchesList();
            }
        }

        void populateMatchesList () {
            JObject x;
            string currentEvent = Preferences.Get(ConstantVars.CURRENT_EVENT_NAME, "");
            if (!String.IsNullOrWhiteSpace (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""))) {
                try {
                    x = (JObject) JObject.Parse (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""));
                    if (x.ContainsKey(currentEvent))
                    {
                        x = (JObject) x[currentEvent];
                    }
                    else
                    {
                        x = new JObject();
                    }
                } catch {
                    Console.WriteLine ("Caught NullRepEx for populateMatchesList");
                    x = new JObject ();
                }
            } else {
                x = new JObject ();
            }
            if (!x.HasValues) {
                matchesList = null;
                listView.ItemsSource = null;
            } else {
                JObject matchesJSON = (JObject) JObject.Parse (Preferences.Get (ConstantVars.APP_DATA_STORAGE, ""))[currentEvent];
                JArray temp = (JArray) matchesJSON["Matches"];
                //Will Contain all items for matches list
                matchesList = new List<MatchesListFormat> ();
                int count;
                try {
                    count = temp.Count;
                } catch {
                    count = -1;
                }

                for (int i = 0; i < count; i++) {
                    JObject match = (JObject) temp[i];
                    string teamTemp = "";
                    try
                    {
                        teamTemp = match["team"].ToString();
                    }
                    catch {}
                    String teamIdentifier = teamTemp;
                    //try
                    //{
                    //    teamIdentifier = teamTemp.Split("-", 2)[MatchFormat.teamNameOrNum].Trim();
                    //}
                    //catch {
                    //    teamIdentifier = teamTemp;
                    //}

                    matchesList.Add (new MatchesListFormat {
                        matchNum = "Match " + match["matchNum"],
                            teamNameAndSide = teamIdentifier + " - " + MatchFormat.matchSideFromEnum ((int) match["side"])
                    });
                }
                listView.ItemsSource = matchesList;
            }
            try {
                matchesView.IsVisible = matchesList.Count > 0;
                sadNoMatch.IsVisible = !matchesView.IsVisible;
            } catch {
                matchesView.IsVisible = false;
                sadNoMatch.IsVisible = true;
            }

        }
    }
}