using System.Collections.Generic;

namespace NutzCode.CloudFileSystem
{
    public class Mappings
    {
        private Dictionary<string, List<string>> _dict=new Dictionary<string, List<string>>();

        public List<string> Get(string s)
        {
            if (_dict.ContainsKey(s))
                return _dict[s];
            return null;
        }

        public void Add(string key, string map)
        {
            List<string> ls;
            if (!_dict.ContainsKey(key))
            {
                ls = new List<string>();
                _dict.Add(key, ls);
            }
            else
                ls = _dict[key];
            if (!ls.Contains(map))
                ls.Add(map);
        }

    }
}
