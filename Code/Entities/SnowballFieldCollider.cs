using Monocle;
using System;

namespace FlavorsHelper.Entities
{
    public class SnowballFieldCollider : Component
    {
		private Collider collider;

		public Action<SnowballField> OnCollide;

		public SnowballFieldCollider(Action<SnowballField> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			this.collider = collider;
			OnCollide = onCollide;
		}

		public bool Check(SnowballField field)
		{
			Collider collider = base.Entity.Collider;
			if (this.collider != null)
			{
				base.Entity.Collider = this.collider;
			}
			bool result = field.CollideCheck(base.Entity);
			base.Entity.Collider = collider;
			return result;
		}
	}
}
