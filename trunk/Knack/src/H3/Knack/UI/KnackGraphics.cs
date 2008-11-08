/*
 *  Copyright 2004, 2005, 2006, 2007, 2008 Riccardo Gerosa.
 *
 *  This file is part of Knack.
 *
 *  Knack is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Knack is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Knack.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace H3.Knack.UI
{
	/// <summary>
	/// Knack graphics utility class.
	/// </summary>
	public sealed class KnackGraphics
	{
		//static Pen LinkLinePen = new Pen(Color.White,2);
		static Pen CreateLinkLinePen = new Pen(Color.White,2);
		static Pen LeftToRightArrowPen = new Pen(Color.FromArgb(255,200,255,200),2);
		//static Pen LeftToRightArrowLightPen = new Pen(Color.FromArgb(255,200,255,200),1);
		static Brush LeftToRightArrowBrush = new SolidBrush(Color.FromArgb(255,200,255,200));
		static Pen TopToBottomArrowPen =  new Pen(Color.FromArgb(255,230,230,255),2);
		static Pen ArrowShadowPen = new Pen(Color.FromArgb(100,0,0,0),3);
		static Brush ArrowShadowBrush = new SolidBrush(Color.FromArgb(100,0,0,0));
		static Brush LinkBrush = new SolidBrush(Color.FromArgb(60,255,255,255));
		
    	static KnackGraphics()
    	{ 
    		CreateLinkLinePen.DashStyle = DashStyle.Dash;
    	}
		
		public static void DrawLeftToRightArrow(Graphics g, int x1, int y1, int x2, int y2) 
		{
			int startLine = 7;
			int endLine = 15;
			int bezierStregth = 10;
			float arrowLength = 7.0f;
			float arrowWidth = 3.2f;
			
			// Shadow
			g.DrawLine(ArrowShadowPen,x1+1,y1+1,x1+startLine+2,y1+1);
			g.DrawLine(ArrowShadowPen,x2-endLine,y2+1,x2+1-8,y2+1);
			g.DrawBezier(ArrowShadowPen,
				x1+startLine+1,y1+1,
				x1+startLine+bezierStregth+1,y1+1,
				x2-endLine-bezierStregth+1,y2+1,
			    x2-endLine+1,y2+1);
			// Head shadow
			PointF[] arrowPointsShadow = { 
				new PointF(x2-5-arrowLength+1,y2-arrowWidth+1),
				new PointF(x2-5+1,y2+1), 
				new PointF(x2-5-arrowLength+1,y2+arrowWidth+1)
			};
			GraphicsPath arrowPathShadow = new GraphicsPath();
			arrowPathShadow.AddPolygon(arrowPointsShadow);
			g.FillPath(ArrowShadowBrush,arrowPathShadow);
			// Line
			g.DrawLine(LeftToRightArrowPen,x1,y1,x1+startLine+1,y1);
			g.DrawLine(LeftToRightArrowPen,x2-endLine-1,y2,x2-8,y2);
			g.DrawBezier(LeftToRightArrowPen,
				x1+startLine,y1,
				x1+startLine+bezierStregth,y1,
				x2-endLine-bezierStregth,y2,
				x2-endLine,y2);
			// Head
			PointF[] arrowPoints = { 
				new PointF(x2-5-arrowLength,y2-arrowWidth),
				new PointF(x2-5,y2), 
				new PointF(x2-5-arrowLength,y2+arrowWidth)
			};
			GraphicsPath arrowPath = new GraphicsPath();
			arrowPath.AddPolygon(arrowPoints);
			g.FillPath(LeftToRightArrowBrush,arrowPath);	           
		}
		
		public static void DrawTopToBottomArrow(Graphics g, int x1, int y1, int x2, int y2) 
		{
			int endLine = (int)Math.Round((y2 - y1) * 0.2);
			if (endLine>10) endLine = 10;
			int bezierStregth = (int)Math.Round((y2 - y1 - endLine*2) * 0.75);

			// Shadow
			g.DrawLine(ArrowShadowPen,x1+1,y1+1,x1+1,y1+endLine+2);
			g.DrawLine(ArrowShadowPen,x2+1,y2-endLine,x2+1,y2+1);
			g.DrawBezier(ArrowShadowPen,
				x1+1,y1+endLine+1,
				x1+1,y1+endLine+bezierStregth+1,
				x2+1,y2-endLine-bezierStregth+1,
				x2+1,y2-endLine+1);
			// Line
			g.DrawLine(TopToBottomArrowPen,x1,y1,x1,y1+endLine+1);
			g.DrawLine(TopToBottomArrowPen,x2,y2-endLine-1,x2,y2);
			g.DrawBezier(TopToBottomArrowPen,
				x1,y1+endLine,
				x1,y1+endLine+bezierStregth,
				x2,y2-endLine-bezierStregth,
				x2,y2-endLine);
		}
		
		public static void DrawSelection(Graphics g, int x1, int y1, int x2, int y2)
		{
			if (x1 > x2) {
				int temp = x1; x1 = x2; x2 = temp;
			}
			if (y1 > y2) {
				int temp = y1; y1 = y2; y2 = temp;
			}
			g.FillRectangle(LinkBrush,x1,y1,x2-x1,y2-y1);
			g.DrawRectangle(CreateLinkLinePen,x1,y1,x2-x1,y2-y1);
		}
		
		public static void DrawLinkingLine(Graphics g, int x1, int y1, int x2, int y2)
		{
			g.DrawLine(CreateLinkLinePen,x1,y1,x2,y2);
		}
	}
		
}
