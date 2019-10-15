﻿using ADK;
using Planetarium.Config;
using Planetarium.Objects;
using Planetarium.Plugins.MinorBodies;
using Planetarium.Renderers;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public class CometsRenderer : BaseRenderer
    {
        private Font fontNames;
        private readonly CometsCalc cometsCalc;
        private readonly ISettings settings;

        private readonly Color colorComet = Color.FromArgb(100, 191, 209, 255);

        public CometsRenderer(CometsCalc cometsCalc, ISettings settings)
        {
            this.cometsCalc = cometsCalc;
            this.settings = settings;

            fontNames = new Font("Arial", 8);
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allComets = cometsCalc.Comets;
            bool isGround = settings.Get<bool>("Ground");
            bool useTextures = settings.Get<bool>("UseTextures");
            double coeff = map.DiagonalCoefficient();
            bool drawComets = settings.Get<bool>("Comets");
            bool drawLabels = settings.Get<bool>("AsteroidsLabels");
            Brush brushNames = new SolidBrush(map.GetColor("ColorCometsLabels"));

            if (drawComets)
            {
                var comets = allComets.Where(a => Angle.Separation(map.Center, a.Horizontal) < map.ViewAngle * coeff);

                foreach (var c in comets)
                {
                    float size = map.GetPointSize(c.Magnitude, maxDrawingSize: 3);

                    if ((int)size > 0)
                    {
                        double ad = Angle.Separation(c.Horizontal, map.Center);

                        if ((!isGround || c.Horizontal.Altitude + c.Semidiameter / 3600 > 0) &&
                            ad < coeff * map.ViewAngle + c.Semidiameter / 3600)
                        {
                            float diam = map.GetDiskSize(c.Semidiameter);

                            PointF p = map.Project(c.Horizontal);
                            PointF t = map.Project(c.TailHorizontal);

                            double tail = map.DistanceBetweenPoints(p, t);

                            if (diam > 5 || tail > 10)
                            {
                                using (var gpComet = new GraphicsPath())
                                {
                                    double rotation = Math.Atan2(t.Y - p.Y, t.X - p.X) + Math.PI / 2;
                                    gpComet.StartFigure();
                                    gpComet.AddArc(p.X - diam / 2, p.Y - diam / 2, diam, diam, (float)Angle.ToDegrees(rotation), 180);
                                    gpComet.AddLines(new PointF[] { gpComet.PathPoints[gpComet.PathPoints.Length - 1], t, gpComet.PathPoints[0] });
                                    gpComet.CloseAllFigures();
                                    using (var brushComet = new PathGradientBrush(gpComet))
                                    {
                                        brushComet.CenterPoint = p;
                                        brushComet.CenterColor = map.GetColor(colorComet);
                                        brushComet.SurroundColors = gpComet.PathPoints.Select(pp => Color.Transparent).ToArray();
                                        g.FillPath(brushComet, gpComet);
                                    }
                                }

                                if (drawLabels)
                                {
                                    map.DrawObjectCaption(fontNames, brushNames, c.Name, p, diam);
                                }
                                map.AddDrawnObject(c);
                            }
                            else if (!map.IsOutOfScreen(p))
                            {
                                g.FillEllipse(new SolidBrush(map.GetColor(Color.White)), p.X - size / 2, p.Y - size / 2, size, size);
                                if (drawLabels)
                                {
                                    map.DrawObjectCaption(fontNames, brushNames, c.Name, p, size);
                                }
                                map.AddDrawnObject(c);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        public override RendererOrder Order => RendererOrder.SolarSystem;
    }
}
