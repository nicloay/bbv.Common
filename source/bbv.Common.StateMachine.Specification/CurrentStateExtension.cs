//-------------------------------------------------------------------------------
// <copyright file="CurrentStateExtension.cs" company="bbv Software Services AG">
//   Copyright (c) 2008-2011 bbv Software Services AG
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace bbv.Common.StateMachine.Specification
{
    using Extensions;

    public class CurrentStateExtension : ExtensionBase<int, int>
    {
        public int CurrentState { get; private set; }

        public override void SwitchedState(IStateMachineInformation<int, int> stateMachine, Internals.IState<int, int> oldState, Internals.IState<int, int> newState)
        {
            this.CurrentState = newState.Id;
        }
    }
}