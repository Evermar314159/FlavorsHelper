using Celeste;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavorsHelper.Entities
{
    public class TouchSwitchCollider : Component
    {
		private Collider collider;

		public Action<TouchSwitch> OnCollide;

		public TouchSwitchCollider(Action<TouchSwitch> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			this.collider = collider;
			OnCollide = onCollide;
		}

		public bool Check(TouchSwitch touchswitch)
		{
			Collider collider = base.Entity.Collider;
			if (this.collider != null)
			{
				base.Entity.Collider = this.collider;
			}
			bool result = touchswitch.CollideCheck(base.Entity);
			base.Entity.Collider = collider;
			return result;
		}
		
	}
}
