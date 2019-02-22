﻿using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Renders Milky Way filled outline on the map
    /// </summary>
    public class MilkyWayRenderer : BaseRenderer
    {
        private readonly IMilkyWayProvider milkyWayProvider;
        private readonly ISettings settings;

        /// <summary>
        /// Primary color to fill outline
        /// </summary>
        private Color colorMilkyWay = Color.FromArgb(20, 20, 20);

        private double minAlpha = 255;
        private double maxAlpha = 10;
        private double minZoom = 90;
        private double maxZoom = 5;
        private double k;
        private double b;

        public MilkyWayRenderer(IMilkyWayProvider milkyWayProvider, ISettings settings)
        {
            this.milkyWayProvider = milkyWayProvider;
            this.settings = settings;

            k = -(minAlpha - maxAlpha) / (maxZoom - minZoom);
            b = -(minZoom * maxAlpha - maxZoom * minAlpha) / (maxZoom - minZoom);
        }

        public override void Render(IMapContext map)
        {
            if (settings.Get<bool>("MilkyWay"))
            {
                double coeff = map.DiagonalCoefficient();
                int alpha = Math.Min((int)(k * map.ViewAngle + b), 255);
                if (alpha > maxAlpha)
                {
                    var smoothing = map.Graphics.SmoothingMode;
                    map.Graphics.SmoothingMode = SmoothingMode.None;

                    for (int i = 0; i < milkyWayProvider.MilkyWay.Count(); i++)
                    {
                        var points = new List<PointF>();
                        bool isOutOfScreen = false;

                        for (int j = 0; j < milkyWayProvider.MilkyWay[i].Count; j++)
                        {
                            var h = milkyWayProvider.MilkyWay[i][j].Horizontal;
                            double ad = Angle.Separation(h, map.Center);

                            if (ad < 90 * coeff)
                            {
                                points.Add(map.Project(h));
                            }
                            else
                            {                                
                                if (!isOutOfScreen && points.Any())
                                {
                                    h = Angle.Intermediate(map.Center, h, 90 * coeff / ad);
                                    points.Add(map.Project(h));
                                    isOutOfScreen = true;
                                }   
                            }
                        }

                        if (points.Count >= 3)
                        {
                            map.Graphics.FillPolygon(new SolidBrush(Color.FromArgb(alpha, colorMilkyWay)), points.ToArray(), FillMode.Winding);
                        }
                    }

                    map.Graphics.SmoothingMode = smoothing;
                }
            }
        }

        public override int ZOrder => 100;
    }
}
