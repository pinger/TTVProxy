using XmlSettings;

namespace TTVProxy
{
    public class SettingManager
    {
        private Settings _settings;

        public SettingManager(string path)
        {
            _settings = new Settings(path);
        }

        public int GetSetting(string section, string key, int defvalue)
        {
            int result;
            if (!int.TryParse(_settings.GetValue(section, key), out result))
            {
                result = defvalue;
                _settings.SetValue(section, key, defvalue.ToString());
            }
            return result;
        }

        public string GetSetting(string section, string key, string defvalue = null)
        {
            string str = _settings.GetValue(section, key);
            if (!string.IsNullOrEmpty(defvalue) && string.IsNullOrEmpty(str) && defvalue != null)
            {
                str = defvalue;
                _settings.SetValue(section, key, defvalue);
            }
            return str;
        }

        public bool GetSetting(string section, string key, bool defvalue)
        {
            bool result;
            if (!bool.TryParse(_settings.GetValue(section, key), out result))
            {
                result = defvalue;
                _settings.SetValue(section, key, defvalue.ToString());
            }
            return result;
        }

        public void SetSetting(string section, string key, object value)
        {
            _settings.SetValue(section, key, value.ToString());
        }
    }
}