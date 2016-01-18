using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using ScoutingApp.Data;
using ScoutingModels.Data;

namespace ScoutingApp.Models
{
    public class HomePageModel : INotifyPropertyChanged
    {
        public ObservableCollection<Team> Teams { get; set; }
        public ObservableCollection<EventGrouping> EventGroupings { set; get; }
        private Team _myTeam;
        public Team MyTeam { get { return _myTeam; }
            set
            {
                if (value != _myTeam)
                {
                    _myTeam = value;
                    OnPropertyChanged();
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
