using System.Windows.Forms;
using GTA;

namespace GTA5CarModifier.Config
{
    public class FlyKeyConfig
    {
        
        public FlyKeyConfig(ScriptSettings settings) 
        {
            UpwardDirection = settings.GetValue("FlyKeyConfig", "UpwardDirection", Keys.T);
            
            DownwardDirection = settings.GetValue("FlyKeyConfig", "DownwardDirection", Keys.Y);
            
            ForwardDirection = settings.GetValue("FlyKeyConfig", "ForwardDirection", Keys.I);
            
            LeftDirection = settings.GetValue("FlyKeyConfig", "LeftDirection", Keys.J);
            
            BackwardDirection = settings.GetValue("FlyKeyConfig", "BackwardDirection", Keys.K);
            
            RightDirection = settings.GetValue("FlyKeyConfig", "RightDirection", Keys.L);
        }

        public Keys UpwardDirection;
        public Keys DownwardDirection;
        public Keys ForwardDirection;
        public Keys LeftDirection;
        public Keys BackwardDirection;
        public Keys RightDirection;
    }
}
