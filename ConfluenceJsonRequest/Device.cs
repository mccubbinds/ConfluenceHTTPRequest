using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfluenceJsonRequest
{
    internal class Device
    {
        public List<Component> ComponentList { get; set; }

        public byte Index { get; set; }
        public string Name { get; set; }

        public Device(byte index, string name)
        {
            Index = index;
            Name = name;
            ComponentList = new List<Component>();
        }

        public Device(byte index, string name, List<Component> componentList)
        {
            Index = index;
            Name = name;
            ComponentList = componentList;
        }

        public void AddComponentToList(byte index, string name, string componentPageTitle)
        {
            ComponentList.Add(new Component(index, name, componentPageTitle));
        }

        public ResponseCode.Response GetComponent(byte index, ref Component componentToReturn)
        {
            ResponseCode.Response responseCode = ResponseCode.Response.UnkownComponent;

            foreach(Component component in ComponentList)
            {
                if(component.Index == index)
                {
                    componentToReturn = component;
                    responseCode = ResponseCode.Response.Success;
                }
            }

            return responseCode;
        }
    }

    internal class Component
    {
        public List<Parameter> ParameterList { get; set; }

        public byte Index { get; set; }
        public string Name { get; set; }
        public string ComponentPageTitle { get; set; }

        public Component(byte index, string name, string componentPageTitle)
        {
            Index = index;
            Name = name;
            ComponentPageTitle = componentPageTitle;
            ParameterList = new List<Parameter>();
        }

        public Component(byte index, string name,  List<Parameter> parameterList)
        {
            Index = index;
            Name = name;
            ParameterList = parameterList;
        }

        public void AddParameterToList(byte index, string name)
        {
            ParameterList.Add(new Parameter(index, name));
        }

        public ResponseCode.Response GetParameter(byte index, ref Parameter parameterToReturn)
        {
            ResponseCode.Response responseCode = ResponseCode.Response.UnkownComponent;

            foreach (Parameter parameter in ParameterList)
            {
                if (parameter.Index == index)
                {
                    parameterToReturn = parameter;
                    responseCode = ResponseCode.Response.Success;
                }
            }

            return responseCode;
        }
    }

    internal class Parameter
    {
        public byte Index { get; set; }
        public string Name { get; set; }

        public Parameter(byte index, string name)
        {
            Index = index;
            Name = name;
        }
    }
}
