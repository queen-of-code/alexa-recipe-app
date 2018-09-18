// -*- coding: utf-8 -*-

// Copyright 2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.

// Licensed under the Amazon Software License (the "License"). You may not use this file except in
// compliance with the License. A copy of the License is located at

//    http://aws.amazon.com/asl/

// or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Newtonsoft.Json.Linq;
using Xunit;

namespace AlexaSmartHomeLambda.Tests
{
    public class AlexaResponseUnitTest
    {
        [Fact]
        public void Test_Response()
        {
            // Arrange
            string alexaResponseString = new AlexaResponse().ToString();

            // Act
            JObject response = JObject.Parse(alexaResponseString);

            // Assert
            Assert.Equal("Alexa", response["event"]["header"]["namespace"].ToString());
            Assert.Equal("Response", response["event"]["header"]["name"].ToString());
        }

        [Fact]
        public void Test_Response_Error()
        {
            // Arrange
            JObject payload_error = new JObject
            {
                { "type", "INVALID_SOMETHING" },
                { "message", "ERROR_MESSAGE" }
            };
            AlexaResponse ar = new AlexaResponse("Alexa", "ErrorResponse");
            ar.SetPayload(payload_error.ToString());

            // Act
            JObject response = JObject.Parse(ar.ToString());
            System.Console.WriteLine(response);
            
            // Assert
            Assert.Equal("ErrorResponse", response["event"]["header"]["name"].ToString());
            Assert.Equal("INVALID_SOMETHING", response["event"]["payload"]["type"].ToString());
            Assert.Equal("ERROR_MESSAGE", response["event"]["payload"]["message"].ToString());
        }

        [Fact]
        public void Test_Discovery()
        {
            // Arrange
            AlexaResponse ar = new AlexaResponse("Alexa.Discovery", "Discover.Response", "endpoint-001");

            JObject capabilityAlexa = JObject.Parse(ar.CreatePayloadEndpointCapability());

            JObject propertyPowerstate = new JObject
            {
                { "name", "powerState" }
            };
            JObject capabilityAlexaPowerController = JObject.Parse(ar.CreatePayloadEndpointCapability("AlexaInterface", "Alexa.PowerController", "3", propertyPowerstate.ToString()));

            JArray capabilities = new JArray
            {
                capabilityAlexa,
                capabilityAlexaPowerController
            };

            ar.AddPayloadEndpoint("test", capabilities.ToString());

            // Act
            JObject response = JObject.Parse(ar.ToString());
            System.Console.WriteLine(response);
            
            // Assert
            Assert.Equal("Alexa.Discovery", response["event"]["header"]["namespace"].ToString());
            Assert.Equal("Discover.Response", response["event"]["header"]["name"].ToString());          
            Assert.Equal("Sample Endpoint", response["event"]["payload"]["endpoints"][0]["friendlyName"].ToString());
            Assert.Equal("AlexaInterface", response["event"]["payload"]["endpoints"][0]["capabilities"][0]["type"].ToString());
            Assert.Equal("Alexa", response["event"]["payload"]["endpoints"][0]["capabilities"][0]["interface"].ToString());
            Assert.Equal("Alexa.PowerController", response["event"]["payload"]["endpoints"][0]["capabilities"][1]["interface"].ToString());
        }
        
        [Fact]
        public void Test_Cookie()
        {
            // Arrange
            AlexaResponse ar = new AlexaResponse();
            // Act
            ar.AddCookie("key", "value");
            JObject response = JObject.Parse(ar.ToString());
            
            // Assert
            Assert.Equal("value", response["event"]["endpoint"]["cookie"]["key"].ToString());
        }

        [Fact]
        public void Test_Cookie_Multiple()
        {
            // Arrange
            AlexaResponse ar = new AlexaResponse();
            
            // Act
            ar.AddCookie("key1", "value1");
            ar.AddCookie("key2", "value2");
            ar.AddCookie("key3", "value3");
            JObject response = JObject.Parse(ar.ToString());
            
            // Assert
            Assert.Equal("value1", response["event"]["endpoint"]["cookie"]["key1"].ToString());
            Assert.Equal("value2", response["event"]["endpoint"]["cookie"]["key2"].ToString());
            Assert.Equal("value3", response["event"]["endpoint"]["cookie"]["key3"].ToString());  
        }

    }

}