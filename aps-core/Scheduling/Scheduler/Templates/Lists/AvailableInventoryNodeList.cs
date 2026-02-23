//using System;
//using System.Collections;
//using PT.Common;
//using ListDataType=PT.Scheduler.AvailableInventoryNode;
//using ListType=PT.Scheduler.AvailableInventoryNodeList;
//
//namespace PT.Scheduler
//{
//	/// <summary>
//	/// ListTemplate
//	/// </summary>
//	[Serializable]
//	public class AvailableInventoryNodeList:System.Runtime.Serialization.ISerializable
//	{
//		#region ISerializable Members
//		public AvailableInventoryNodeList(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
//		{
//			PT.Common.Serialization.SetProperties(info,this,typeof(AvailableInventoryNodeList));
//		}
//		public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
//		{
//			PT.Common.Serialization.GetProperties(info,this,typeof(AvailableInventoryNodeList),false);
//		}
//		#endregion
//
//		public AvailableInventoryNodeList()
//		{
//		}
//
//		[Serializable]
//			public class Node:System.Runtime.Serialization.ISerializable
//		{
//			#region ISerializable Members
//			public Node(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
//			{
//				PT.Common.Serialization.SetProperties(info,this,typeof(Node));
//			}
//			public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
//			{
//				PT.Common.Serialization.GetProperties(info,this,typeof(Node),false);
//			}
//			#endregion
//
//			internal Node previous;
//			internal Node next;
//			ListType list;
//			ListDataType data;
//
//			public Node(ListDataType data, Node previous, Node next, ListType list)
//			{
//				this.data=data;
//				this.list=list;
//
//				this.previous=previous;
//				this.next=next;
//
//				if(previous!=null)
//				{
//					previous.next=this;
//					if(previous.list!=list)
//					{
//						throw new ListsDontMatchException();
//					}
//				}
//
//				if(next!=null)
//				{
//					next.previous=this;
//					if(next.list!=list)
//					{
//						throw new ListsDontMatchException();
//					}
//				}
//			}
//
//			public ListDataType Data
//			{
//				get
//				{
//					return data;
//				}
//			}
//
//			public Node Previous
//			{
//				get
//				{
//					return previous;
//				}
//			}
//
//			public Node Next
//			{
//				get
//				{
//					return next;
//				}
//			}
//
//			internal ListType List
//			{
//				get
//				{
//					return list;
//				}
//			}
//		}
//
//		Node first=null;
//		public Node First
//		{
//			get
//			{
//				return first;
//			}
//		}
//
//		Node last=null;
//		public Node Last
//		{
//			get
//			{
//				return last;
//			}
//		}
//
//		int count;
//
//		public int Count
//		{
//			get
//			{
//				return count;
//			}
//		}
//
//		/// <summary>
//		/// Add element after the specified node.
//		/// </summary>
//		/// <param name="data">The element to add to the list.</param>
//		/// <param name="after">The element will be added after this node. The node must be a member of this list. If you specify null, the element will be added to the front of the list.</param>
//		/// <returns>The node the element is stored in.</returns>
//		public Node Add(ListDataType data, Node after)
//		{
//			Node newNode;
//
//			if(after==null)
//			{
//				newNode=new Node(data, null, first, this);
//				first=newNode;
//				if(last==null)
//				{
//					last=first;
//				}
//			}
//			else
//			{
//				newNode=new Node(data, after, after.next, this);
//
//				if(newNode.Next==null)
//				{
//					last=newNode;
//				}
//			}
//
//			count++;
//			return newNode;
//		}
//
//		/// <summary>
//		/// Add element to the end of the list.
//		/// </summary>
//		/// <param name="data">The element to add to the list.</param>
//		/// <returns></returns>
//		public Node Add(ListDataType data)
//		{
//			return Add(data, this.Last);
//		}
//
//		/// <summary>
//		/// Add element to the front of the list.
//		/// </summary>
//		/// <param name="data">The element to add to the list.</param>
//		/// <returns>The node the element is stored in.</returns>
//		public Node AddFront(ListDataType data)
//		{
//			return Add(data, null);
//		}
//
//		/// <summary>
//		/// Add element to the end of the list.
//		/// </summary>
//		/// <param name="data">The element to add to the list.</param>
//		/// <returns>The node the element is stored in.</returns>
//		public Node AddEnd(ListDataType data)
//		{
//			return Add(data, this.Last);
//		}
//
//		public virtual void Clear()
//		{
//			first=null;
//			last=null;
//			count=0;
//		}
//
//		public void Remove(Node node)
//		{
//			if(node.List!=this)
//			{
//				throw new ListsDontMatchException();
//			}
//
//			if(node==first && node==last)
//			{
//				first=null;
//				last=null;
//			}
//			else if(node==first)
//			{
//				first=first.Next;
//				first.previous=null;
//			}
//			else if(node==last)
//			{
//				last=last.previous;
//				last.next=null;
//			}
//			else
//			{
//				node.previous.next=node.next;
//				node.next.previous=node.previous;
//			}
//
//#if DEBUG
//			node.previous=null;
//			node.next=null;
//#endif
//
//			count--;
//		}
//
//		public class ListException:CommonException
//		{
//			public ListException(string msg):base(msg)
//			{
//			}
//		}
//
//		public class ListsDontMatchException:ListException
//		{
//			internal ListsDontMatchException():base("The lists don't match.")
//			{
//			}
//		}
//
//		public ArrayList CreateArrayListShallowCopy()
//		{
//			ArrayList al=new ArrayList();
//
//			Node current=this.First;
//
//			while(current!=null)
//			{
//				al.Add(current.Data);
//				current=current.Next;
//			}
//
//			return al;
//		}
//	}
//}

