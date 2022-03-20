using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBox
{
    public class ConfigWorkSpaceDir
    {
        public ObservableCollection<string> LocalURIs { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> RemoteURIs { get; set; } = new ObservableCollection<string>();
    }
}
