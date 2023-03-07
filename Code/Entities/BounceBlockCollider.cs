using Monocle;
using System;

namespace FlavorsHelper.Entities
{
    public class BounceBlockCollider : Component
    {
		private Collider collider;

		public Action<CustomBounceBlock> OnCollide;

		public BounceBlockCollider(Action<CustomBounceBlock> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			this.collider = collider;
			OnCollide = onCollide;
		}

		public bool Check(CustomBounceBlock block)
		{
			Collider collider = base.Entity.Collider;
			if (this.collider != null)
			{
				base.Entity.Collider = this.collider;
			}
			bool result = block.CollideCheck(base.Entity);
			base.Entity.Collider = collider;
			return result;
		}
	}
}
