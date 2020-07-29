using System;

namespace AwesomeTechnologies.Utility
{
    public class AwesomeTechnologiesScriptOrder : Attribute
    {
        public int Order;

        public AwesomeTechnologiesScriptOrder(int order)
        {
            Order = order;
        }
    }
}