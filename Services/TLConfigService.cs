using DFTelegram.Helper;
using DFTelegram.Models;
using SqlSugar;
using System.Threading;

namespace DFTelegram.Services
{
    public class TLConfigService
    {
        private readonly SemaphoreSlim _signal;
        private string _verificationCode;
        public TLConfigService()
        {
            _signal = new SemaphoreSlim(0);
            _verificationCode = string.Empty;
        }

        public void SetVerificationCode(string verificationCode)
        {
            _verificationCode = verificationCode;
            _signal.Release();
        }

#nullable disable
        public string Config(string what)
        {
            string[] sections = new string[] { "Telegram", what };
            switch (what)
            {
                case "session_pathname":
                case "api_id":
                case "api_hash":
                case "phone_number": return AppsettingsHelper.app(sections);
                case "verification_code":
                    _signal.Wait();
                    return _verificationCode;
                default: return null;
            }
        }
#nullable restore
    }
}