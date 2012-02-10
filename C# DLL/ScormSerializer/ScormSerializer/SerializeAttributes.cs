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

namespace ScormSerialization
{
    [AttributeUsage(AttributeTargets.All)]
    public class WriteOnlyAttribute : System.Attribute
    {
      
    }

    [AttributeUsage(AttributeTargets.All)]
    public class ReadOnlyAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.All)]
    public class IgnoreAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.All)]
    public class PriorityAttribute : System.Attribute
    {
        public PriorityAttribute(int i) { Priority = i; }
        public int Priority;
    }
}
