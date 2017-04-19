using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Siemplify.Common;
using Siemplify.Common.Extensions;

using Siemplify.Integrations.IronPort.Common;

namespace Siemplify.Integrations.IronPort
{
    public class IronPortMetadataProvider : MetadataProviderBase
    {
        public const string ProviderIdentifier = "IronPort";

        private List<ModuleSettingsProperty> _requiredSettings = null;

        public IronPortMetadataProvider()
        {
            ProviderIcon = "Siemplify.Integrations.IronPort.Resources.ironport.jpg";
        }

        public override Stream IconStream
        {
            get
            {
                var data = Convert.FromBase64String(IconBase64);
                var ms = new MemoryStream(data);
                return ms;
            }
        }

        public override string Description
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("http://www.cisco.com/c/en/us/products/security/email-security/index.html");
                sb.AppendLine(@"Protect against advanced email threats");
                return sb.ToString();
            }
        }

        public override string DisplayName
        {
            get { return "IronPort"; }
        }

        public override string Identifier
        {
            get { return "IronPort"; }
        }

        public override List<ModuleSettingsProperty> RequiredSettings
        {
            get
            {
                if (null == _requiredSettings)
                {
                    _requiredSettings = new List<ModuleSettingsProperty>();

                    _requiredSettings.Add(new ModuleSettingsProperty
                    {
                        ModuleName = Identifier,
                        PropertyName = Settings.IronPortHost,
                        PropertyDisplayName = Settings.IronPortHost,
                        PropertyType = ParamTypeEnum.String
                    });

                    _requiredSettings.Add(new ModuleSettingsProperty
                    {
                        ModuleName = Identifier,
                        PropertyName = Settings.IronPortUserName,
                        PropertyDisplayName = Settings.IronPortUserName,
                        PropertyType = ParamTypeEnum.String
                    });

                    _requiredSettings.Add(new ModuleSettingsProperty
                    {
                        ModuleName = Identifier,
                        PropertyName = Settings.IronPortKey,
                        PropertyDisplayName = Settings.IronPortKey,
                        PropertyType = ParamTypeEnum.Password
                    });
                }

                return _requiredSettings;
            }
        }

        public override async Task Test(Dictionary<string, string> paramsWithValues)
        {
            var ironportHost = paramsWithValues.GetOrDefault(Settings.IronPortHost);
            if (ironportHost.IsEmpty())
            {
                throw new Exception(string.Format("Not found <{0}> Field.", Settings.IronPortHost));
            }
            var ironportUser = paramsWithValues.GetOrDefault(Settings.IronPortUserName);
            if (ironportUser.IsEmpty())
            {
                throw new Exception(string.Format("Not found <{0}> Field.", Settings.IronPortUserName));
            }
            var ironportKey = paramsWithValues.GetOrDefault(Settings.IronPortKey);
            if (ironportKey.IsEmpty())
            {
                throw new Exception(string.Format("Not found <{0}> Field.", Settings.IronPortKey));
            }

            IronPortController ironportClient = new IronPortController(ironportHost, ironportUser, ironportKey);

            await Task.Factory.StartNew(() =>
               {
                   var result = ironportClient.GetIronPortVersion();
                   return result;
               });

        }
    }
}
