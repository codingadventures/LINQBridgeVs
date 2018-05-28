#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using BridgeVs.Shared.Common;
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Diagnostics;

namespace BridgeVs.Shared.Logging
{
    public sealed class RavenWrapper
    {
        private static Lazy<RavenWrapper> _instance = new Lazy<RavenWrapper>(() => new RavenWrapper());

        public static RavenWrapper Instance => _instance.Value;

        /// <summary>
        /// The client id associated with the Sentry.io account.
        /// </summary>
        private const string RavenClientId = "https://e187bfd9b1304311be6876ac9036956d:6580e15fc2fd40899e5d5b0f28685efc@sentry.io/1189532";

        private RavenClient _ravenClient;

        public static string VisualStudioVersion;

        private RavenWrapper()
        {
#if !TEST
            Func<IRequester, IRequester> removeUserId = new Func<IRequester, IRequester>(req =>
            {
                HttpRequester request = req as HttpRequester;
                //GDPR compliant, no personal data sent: no server name, no username stored, no ip address
                request.Data.JsonPacket.ServerName = "linqbridgevs";
                request.Data.JsonPacket.Contexts.Device.Name = "linqbridgevs"; 
                request.Data.JsonPacket.User.Username = "linqbridgevs-" + DateTime.Now.ToLongTimeString();
                request.Data.JsonPacket.Release = "1.4.6"; //read it from somewhere
                request.Data.JsonPacket.User.IpAddress = "0.0.0.0";
                return request;
            });

            Action<Exception> onSendError = new Action<Exception>(ex =>
            {
                Trace.WriteLine("Error sending report to Sentry.io");
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
            });

            _ravenClient = new RavenClient(RavenClientId)
            {
                BeforeSend = removeUserId,
                ErrorOnCapture = onSendError,
                Timeout = TimeSpan.FromMilliseconds(500) //should fail early if it can't send a message
            };
#endif
        }

        [Conditional("DEPLOY")]
        public void Capture(Exception exception, ErrorLevel errorLevel = ErrorLevel.Error, string message = "")
        {
            if (!CommonRegistryConfigurations.IsErrorTrackingEnabled(VisualStudioVersion))
            {
                return;
            }

            var sentryEvent = new SentryEvent(exception)
            {
                Message = message,
                Level = errorLevel
            };

            sentryEvent.Tags.Add("Visual Studio Version", VisualStudioVersion);

            _ravenClient.Capture(sentryEvent);
        }
    }
}