/***********************************************************************************************************************
 *
 * Unity-SCORM Integration Toolkit Version 1.0 Beta
 * ==========================================
 *
 * Copyright (C) 2011, by ADL (Advance Distributed Learning). (http://www.adlnet.gov)
 * http://www.adlnet.gov/UnityScormIntegration/
 *
 ***********************************************************************************************************************
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 *
 **********************************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScormSerialization;
namespace ScormSerializerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                //Serialize a cocdtype to a scorm API
                //create an object to handle the get set calls
                InMemoryScormSimulator2004 sim2004 = new InMemoryScormSimulator2004();
                InMemoryScormSimulator1_2 sim12 = new InMemoryScormSimulator1_2();

                Scorm1_2.DataModel model12 = ScormSerialization.ScormDatamodelCommandParser1_2.GetTestData1_2();
                ScormSerializer serialize12 = new ScormSerializer(model12);
                serialize12.Serialize(sim12);

                ScormVersionConversion.DataModel.Translate(new Scorm1_2.DataModel());
                ScormVersionConversion.DataModel.Translate(new Scorm2004.DataModel());

                Console.WriteLine("\n*****Translated to 2004*****\n");
                Scorm2004.DataModel model2004 = ScormVersionConversion.DataModel.Translate(model12);
                //create a serializer for the lr object
                ScormSerializer serialize2004 = new ScormSerializer(model2004);
                //serialize the lr object to the InMemoryScormSimulator
                serialize2004.Serialize(sim2004);

                Console.WriteLine("\n*****Translated Back to 12*****\n");

                model12 = ScormVersionConversion.DataModel.Translate(model2004);
                serialize12 = new ScormSerializer(model12);
                serialize12.Serialize(sim12);

            }
        }
    }
}
