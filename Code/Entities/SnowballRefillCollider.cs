using Celeste;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavorsHelper.Entities
{
    [Tracked]
    public class SnowballRefillCollider : Component
    {
		private Collider collider;

		public Action<SnowballRefill> OnCollide;

		public SnowballRefillCollider(Action<SnowballRefill> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			this.collider = collider;
			OnCollide = onCollide;
		}

		public bool Check(SnowballRefill refill)
		{
			Collider collider = base.Entity.Collider;
			if (this.collider != null)
			{
				base.Entity.Collider = this.collider;
			}
			bool result = refill.CollideCheck(base.Entity);
			base.Entity.Collider = collider;
			return result;
		}
	}
}
