using ECM.Controllers;

namespace ECM.Walkthrough.CustomCharacterController
{
    /// <summary>
    /// Example of a custom character controller.
    ///
    /// This show how to create a custom character controller extending one of the included 'Base' controller. 
    /// </summary>

    public class MyCharacterController : BaseCharacterController
    {
        protected override void Animate()
        {
            // Add animator related code here...
        }
    }
}
