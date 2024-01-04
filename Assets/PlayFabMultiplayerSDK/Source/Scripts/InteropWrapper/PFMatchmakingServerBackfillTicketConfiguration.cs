/*
 * PlayFab Unity SDK
 *
 * Copyright (c) Microsoft Corporation
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

namespace PlayFab.Multiplayer.InteropWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PFMatchmakingServerBackfillTicketConfiguration
    {
        public PFMatchmakingServerBackfillTicketConfiguration(
            uint timeoutInSeconds,
            string queueName,
            List<PFMatchmakingMatchMember> members,
            PFMultiplayerServerDetails serverDetails)
        {
            this.TimeoutInSeconds = timeoutInSeconds;
            this.QueueName = queueName;
            this.Members = members;
            this.ServerDetails = serverDetails;
        }

        internal unsafe Interop.PFMatchmakingServerBackfillTicketConfiguration* ToPointer(DisposableCollection disposableCollection)
        {
            Interop.PFMatchmakingServerBackfillTicketConfiguration interopPtr = new Interop.PFMatchmakingServerBackfillTicketConfiguration();

            interopPtr.timeoutInSeconds = this.TimeoutInSeconds;

            UTF8StringPtr queueNamePtr = new UTF8StringPtr(this.QueueName, disposableCollection);
            interopPtr.queueName = queueNamePtr.Pointer;

            interopPtr.memberCount = (uint)this.Members.Count;

            if (this.Members.Count > 0)
            {
                Interop.PFMatchmakingMatchMember[] matchMembers = new Interop.PFMatchmakingMatchMember[this.Members.Count];
                for (int i = 0; i < this.Members.Count; i++)
                {
                    matchMembers[i] = *this.Members[i].ToPointer(disposableCollection);
                }

                fixed (Interop.PFMatchmakingMatchMember* matchmakingMembersArray = &matchMembers[0])
                {
                    interopPtr.members = matchmakingMembersArray;
                }
            }
            else
            {
                interopPtr.members = null;
            }

            if (this.ServerDetails != null)
            {
                interopPtr.serverDetails = this.ServerDetails.ToPointer(disposableCollection);
            }
            else
            {
                interopPtr.serverDetails = null;
            }

            return (Interop.PFMatchmakingServerBackfillTicketConfiguration*)Converters.StructToPtr<Interop.PFMatchmakingServerBackfillTicketConfiguration>(interopPtr, disposableCollection);
        }

        public uint TimeoutInSeconds { get; set; }

        public string QueueName { get; set; }

        public List<PFMatchmakingMatchMember> Members { get; set; }
        
        public PFMultiplayerServerDetails ServerDetails { get; set; }
    }
}
