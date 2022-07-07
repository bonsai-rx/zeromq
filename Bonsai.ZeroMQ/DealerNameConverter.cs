using Bonsai.Expressions;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.ZeroMQ
{
    public class DealerNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                if (workflowBuilder != null)
                {
                    var dealerNames = (from builder in workflowBuilder.Workflow.Descendants()
                                        let dealer = ExpressionBuilder.GetWorkflowElement(builder) as CreateDealer
                                        where dealer != null && !string.IsNullOrEmpty(dealer.Name)
                                        select dealer.Name)
                    .Distinct()
                    .ToList();
                    return new StandardValuesCollection(dealerNames);
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
