﻿using System;
using System.Collections.Generic;
using IEnumerable = System.Collections.IEnumerable;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Recellection.Code.Utility.Events;
using Recellection.Code.Controllers;

namespace Recellection.Code.Models
{
	/// TODO: Unit tests for events
	[TestFixture]
	public class GraphTest
	{
		private Graph g;
		
		private int size;
		
		private BaseBuilding bb = new BaseBuilding("test", 0, 0, new Player(), new LinkedList<Tile>());
		private Building b1 = new TestBuilding();
		private Building b2 = new TestBuilding();
		private Building b3 = new TestBuilding();
		
		[SetUp]
		public void Init()
		{
			size = 0;
			g = new Graph(bb);
			size += 1;
		}
	
		[Test]
		public void AddAndCount()
		{
			g.Add(b1);
			Assert.AreEqual(++size, g.CountBuildings());
			g.Add(b2);
			Assert.AreEqual(++size, g.CountBuildings());
			g.Add(b3);
			Assert.AreEqual(++size, g.CountBuildings());
			
			// Adding same fromBuilding twice should be ignored

			Assert.Throws<ArgumentException>(delegate { g.Add(b1); });
			Assert.AreEqual(size, g.CountBuildings());
		}
		
		[Test]
		public void Remove()
		{
			g.Add(b1);
			Assert.AreEqual(++size, g.CountBuildings());

			g.Remove(b1);
			Assert.AreEqual(--size, g.CountBuildings());

			g.Remove(b1);
			Assert.AreEqual(size, g.CountBuildings());
		}
		
		[Test]
		public void SetWeight()
		{
			g.Add(b1);
			g.SetWeight(b1, 100);
			++size;
			Assert.AreEqual(size, g.CountBuildings());

			Assert.Throws<GraphLessBuildingException>(delegate { g.SetWeight(b2, 200); });
		}
		
		[Test]
		public void GetWeight()
		{
			g.Add(b1);
			g.SetWeight(b1, 100);
			Assert.AreEqual(100, g.GetWeight(b1));
			g.SetWeight(b1, 200);
			Assert.AreEqual(200, g.GetWeight(b1));
		}
		
		[Test]
		public void GetWeightFactor()
		{
			g.Add(b1);
			g.Add(b2);
			g.Add(b3);
			g.SetWeight(b1, 10);
			g.SetWeight(b2, 20);
			g.SetWeight(b3, 70);
			
			Assert.AreEqual(0.1, Math.Round(g.GetWeightFactor(b1), 1));
			Assert.AreEqual(0.2, Math.Round(g.GetWeightFactor(b2), 1));
			Assert.AreEqual(0.7, Math.Round(g.GetWeightFactor(b3), 1));
		}
		
		[Test]
		public void HasBuilding()
		{
			Assert.False(g.HasBuilding(b1));
			g.Add(b1);
			Assert.True(g.HasBuilding(b1));
		}
		
		[Test]
		public void GetWeightOfNonExistingBuilding()
		{
			Assert.Throws<ArgumentException>(delegate{ g.GetWeight(b1); });
			
			g.Add(b1);
			g.Remove(b1);
			
			Assert.Throws<ArgumentException>(delegate { g.GetWeight(b1); });
		}

		[Test]
		public void GetBuildings()
		{
			Graph g2 = new Graph(bb);
			g.Add(b1);
			g.SetWeight(b1, 1);
			g2.Add(b1);
			g2.SetWeight(b1, 1);
			
			g.Add(b2);
			g.SetWeight(b2, 2);
			g2.Add(b2);
			g2.SetWeight(b2, 2);
			
			g.Add(b3);
			g.SetWeight(b3, 3);
			g2.Add(b3);
			g2.SetWeight(b3, 3);
			
			IEnumerable<Building> it = g.GetBuildings();
			foreach(Building b in it)
			{
				g2.SetWeight(b, g.GetWeight(b)+10);
			}
						
			Assert.AreEqual(11, g2.GetWeight(b1));
			Assert.AreEqual(12, g2.GetWeight(b2));
			Assert.AreEqual(13, g2.GetWeight(b3));
		}

		public void observed(Object g, Event<Building> e)
		{
			wasNotified = true;
		}
		
		private bool wasNotified = false;
		
		[Test]
		public void Publish()
		{
			g.weightChanged += this.observed;
			g.Add(b1);
			g.SetWeight(b1, 150);
			Assert.IsTrue(wasNotified);
		}
	}
}
