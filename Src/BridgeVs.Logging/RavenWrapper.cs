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

using SharpRaven;
using SharpRaven.Data;
using System;
using System.Diagnostics;

namespace BridgeVs.Logging
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

        private RavenWrapper()
        {
#if DEPLOY
            Func<Requester, Requester> removeUserId = new Func<Requester, Requester>(req =>
            {
                //GDPR compliant, no server name or username stored
                req.Packet.ServerName = string.Empty;
                req.Packet.User.Username = string.Empty;
                return req;
            });

            Action<Exception> onSendError = new Action<Exception>(ex =>
            {
                Log.Configure("entry", "AllProjects");
                Log.Write(ex, "Error sending the exception to Sentry.");
            });
            _ravenClient = new RavenClient(RavenClientId);
            _ravenClient.BeforeSend = removeUserId;
            _ravenClient.ErrorOnCapture = onSendError;
#endif
        }

        [Conditional("DEPLOY")]
        public void Capture(Exception exception, string vsVersion, ErrorLevel errorLevel = ErrorLevel.Error, string message = "")
        {
            var sentryEvent = new SentryEvent(exception)
            {
                Message = message,
                Level = errorLevel
            };
            //Log LINQBridgeVs detail
            sentryEvent.Fingerprint.Add(vsVersion);

            _ravenClient.Capture(sentryEvent);
        }
    }
}