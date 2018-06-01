using GameTransition.Utility;
using Malee;
using System;
using UnityEngine;

namespace GameTransition {
	[Serializable]
	public class InvokeDescriptorList : ReorderableArray<InvokeDescriptor> { }

	[Serializable]
	public class GameObjectMultiInvokeHolder {
		[SerializeField, Reorderable( singleLine = false, elementNameProperty = "ToString" )]
		public InvokeDescriptorList Descriptors;

		public GameObject InvokeGO { set; get; }

		public void Invoke() {
			if( !InvokeGO ) {
				return;
			}

			foreach( var descrptor in Descriptors ) {
				if( string.IsNullOrEmpty( descrptor.ComponentType ) ) {
					TypeHelper.InvokeMethodOrProperty( descrptor, InvokeGO );
				}
				else {
					var component = InvokeGO.GetComponent( descrptor.ComponentOwner );
					TypeHelper.InvokeMethodOrProperty( descrptor, component );
				}
			}
		}
	}
}
