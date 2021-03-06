//-------------------------------------------------------------------------------
// <copyright file="DefaultEventMessageFactoryTest.cs" company="bbv Software Services AG">
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

namespace bbv.Common.DistributedEventBroker.Factories
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using bbv.Common.DistributedEventBroker.Messages;
    using Xunit;
    using Xunit.Extensions;

    public class DefaultEventMessageFactoryTest
    {
        private readonly DefaultEventMessageFactory testee;

        public DefaultEventMessageFactoryTest()
        {
            this.testee = new DefaultEventMessageFactory();
        }

        public static IEnumerable<object[]> InitializerInput
        {
            get
            {
                var distributedBrokerId = "DistributedEventBroker";
                yield return new object[] { distributedBrokerId, Guid.NewGuid(), "topic://Simple", EventArgs.Empty, };
                yield return new object[] { distributedBrokerId, Guid.NewGuid(), "topic://Complex", new CancelEventArgs(), };
            }
        }

        [Fact]
        public void CreateEventFiredMessage_MustCreateEventFiredMessage()
        {
            Assert.IsType<EventFired>(this.testee.CreateEventFiredMessage(x => { }));
        }

        [Theory]
        [PropertyData("InitializerInput")]
        public void CreateEventFiredMessage_MustAssignPropertiesWithInitializer(string distributedIdentification, Guid identification, string topic, EventArgs eventArgs)
        {
            var result = this.testee.CreateEventFiredMessage(x =>
                                                    {
                                                        x.DistributedEventBrokerIdentification =
                                                            distributedIdentification;
                                                        x.EventBrokerIdentification = identification.ToString();
                                                        x.Topic = topic;
                                                        x.EventArgs = eventArgs.ToString();
                                                    });

            Assert.Equal(distributedIdentification, result.DistributedEventBrokerIdentification);
            Assert.Equal(identification.ToString(), result.EventBrokerIdentification);
            Assert.Equal(topic, result.Topic);
            Assert.Equal(eventArgs.ToString(), result.EventArgs);
        }
    }
}