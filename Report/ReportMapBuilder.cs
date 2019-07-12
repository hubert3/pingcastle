﻿//
// Copyright (c) Ping Castle. All rights reserved.
// https://www.pingcastle.com
//
// Licensed under the Non-Profit OSL. See LICENSE file in the project root for full license information.
//
using PingCastle.Data;
using PingCastle.Healthcheck;
using PingCastle.template;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PingCastle.Report
{
	public class ReportHealthCheckMapBuilder : ReportBase
    {
		protected PingCastleReportCollection<HealthcheckData> Report = null;
		protected OwnerInformationReferences EntityData = null;

		public ReportHealthCheckMapBuilder(PingCastleReportCollection<HealthcheckData> consolidation, OwnerInformationReferences ownerInformationReferences)
        {
            this.Report = consolidation;
			EntityData = ownerInformationReferences;
			FullNodeMap = true;
        }
		public ReportHealthCheckMapBuilder(PingCastleReportCollection<HealthcheckData> consolidation) : this(consolidation, null)
		{
		}


        public delegate void GraphLogging(string message);

        public GraphLogging Log { get; set; }

        public MigrationChecker migrationChecker { get; set; }

		public string CenterDomainForSimpliedGraph { get; set; }

		public bool FullNodeMap { get; set; }

        // build a model & cache it
        GraphNodeCollection _nodes;
        protected GraphNodeCollection Nodes
        {
            get
            {
                if (_nodes == null)
                    _nodes = GraphNodeCollection.BuildModel(Report, EntityData);
                return _nodes;
            }
        }

		protected override void GenerateTitleInformation()
		{
			Add("PingCastle AD Map ");
			Add(DateTime.Now.ToString("yyyy-MM-dd"));
			Add(" (");
			Add(Nodes.Count);
			Add(" domains)");
		}

		protected override void GenerateHeaderInformation()
		{
			AddBeginStyle();
			AddLine(ReportBase.GetStyleSheetTheme());
			Add(@"
.legend_carto 
{
	position: absolute;
	top: 65px;
	left: 0px;
}
.legend_criticalscore {
    background: #A856AA;
    border: #19231a;
    border-style: solid;
    border-width: 1px;
    padding: 5px;
}
.legend_superhighscore {
    background: #E75351;
    border: #19231a;
    border-style: solid;
    border-width: 1px;
    padding: 5px;
}
.legend_highscore {
    background: #FA9426;
    border: #19231a;
    border-style: solid;
    border-width: 1px;
    padding: 5px;
}
.legend_mediumscore {
    background: #FDC334;
    border: #19231a;
    border-style: solid;
    border-width: 1px;
    padding: 5px;
}

.legend_lowscore {
    background: #74C25C;
    border: #19231a;
    border-style: solid;
    border-width: 1px;
    padding: 5px;
}
.legend_unknown {
    background: #ffffff;
    border: #a352cc;
    border-style: solid;
    border-width: 1px;
    padding: 5px;
}
.network-area
{
height: 100%;
min-height: 100%;
border-width:1px;
}
");
			AddLine(TemplateManager.LoadVisCss());
			AddLine(@"</style>");
		}

		protected override void GenerateBodyInformation()
		{
			GenerateNavigation("Active Directory map " + (FullNodeMap?"full":"simple"), null, DateTime.Now);
			GenerateAbout(@"
<p><strong>Generated by <a href=""https://www.pingcastle.com"">Ping Castle</a> all rights reserved</strong></p>
<p>Open source components:</p>
<ul>
<li><a href=""https://getbootstrap.com/"">Bootstrap</a> licensed under the <a href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a href=""https://datatables.net/"">DataTables</a> licensed under the <a href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a href=""https://popper.js.org/"">Popper.js</a> licensed under the <a href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a href=""https://jquery.org"">JQuery</a> licensed under the <a href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
<li><a href=""http://visjs.org/"">vis.js</a> licensed under the <a href=""https://tldrlegal.com/license/mit-license"">MIT license</a></li>
</ul>
");
			Add(@"
<noscript>
	<div class=""alert alert-alert"">
		<p><strong>This report requires javascript.</strong></p>
	</div>
</noscript>
<!-- Modal -->
<div class=""modal"" id=""loadingModal"" role=""dialog"">
    <div class=""modal-dialog"">
        <!-- Modal content-->
        <div class=""modal-content"">
            <div class=""modal-header"">
                <h4 class=""modal-title"">Loading ...</h4>
            </div>
            <div class=""modal-body"">
                <div class=""progress"">
                    <div class=""progress-bar"" role=""progressbar"" aria-valuenow=""0"" aria-valuemin=""0"" aria-valuemax=""100"">
                        0%
                    </div>
                </div>
            </div>
        </div>

    </div>
</div>
<div id=""mynetwork"" class=""network-area""></div>

<div class=""legend_carto"">
    Legend: <br>
    <i class=""legend_criticalscore"">&nbsp;</i> score=100<br>
    <i class=""legend_superhighscore"">&nbsp;</i> score &lt; 100<br>
    <i class=""legend_highscore"">&nbsp;</i> score &lt; 70<br>
    <i class=""legend_mediumscore"">&nbsp;</i> score &lt; 50<br>
    <i class=""legend_lowscore"">&nbsp;</i> score &lt; 30<br>
    <i class=""legend_unknown"">&nbsp;</i> score unknown
</div>
");
		}

		protected override void GenerateFooterInformation()
		{
			AddBeginScript();
			AddLine(TemplateManager.LoadVisJs());
			Add(@"
    function getTooltipHtml(d) {
        var output = '<b>' + d.name + '</b>';
        if (d.FullEntityName != null) {
        output += '<br/>Entity: ' + d.FullEntityName;
    } else {
            if (d.BU != null) {
        output += '<br/>BU: ' + d.BU;
    }
            if (d.entity != null) {
        output += '<br/>Entity: ' + d.entity;
    }
        }
        if (d.forest != null) {
        output += '<br/>Forest: ' + d.forest;
    }
        if (d.score != null) {
			output += '<br/>Score: ' + d.score;
			output += '<ul>';
			output += '<li>StaleObjects: ' + d.staleObjectsScore;
			output += '<li>Privilegied Group: ' + d.privilegiedGroupScore;
			output += '<li>Trust: ' + d.trustScore;
			output += '<li>Anomaly: ' + d.anomalyScore;
			output += '</ul>';
        }
        return output;
    }

    var colors = {
                        'default': {background: '#CCCCCC', border: '#212121', highlight: {background: '#CCCCCC', border: '#212121' } },
        criticalscore: {background: '#A856AA', border: '#19231a', highlight: {background: '#A856AA', border: '#19231a' } },
        superhighscore: {background: '#E75351', border: '#19231a', highlight: {background: '#E75351', border: '#19231a' } },
        highscore: {background: '#FA9426', border: '#19231a', highlight: {background: '#FA9426', border: '#19231a' } },
        mediumscore: {background: '#FDC334', border: '#19231a', highlight: {background: '#FDC334', border: '#19231a' } },
        lowscore: {background: '#74C25C', border: '#19231a', highlight: {background: '#74C25C', border: '#19231a' } },
        ucriticalscore: {background: '#f3f3f3', border: '#B1B1B1', highlight: {background: '#A856AA', border: '#19231a' } },
        usuperhighscore: {background: '#f3f3f3', border: '#B1B1B1', highlight: {background: '#E75351', border: '#19231a' } },
        uhighscore: {background: '#f3f3f3', border: '#B1B1B1', highlight: {background: '#FA9426', border: '#19231a' } },
        umediumscore: {background: '#f3f3f3', border: '#B1B1B1', highlight: {background: '#FDC334', border: '#19231a' } },
        ulowscore: {background: '#f3f3f3', border: '#B1B1B1', highlight: {background: '#74C25C', border: '#19231a' } },
        unknown: {background: '#ffffff', border: '#a352cc', highlight: {background: '#ffffff', border: '#a352cc' } }
    };

function reshape(tree) {

    var nodes = [], edges = [], id = [0];
    function toNode(id, n, parentId, parentName, level, direction, nodes, edges) {
        id[0]++;
        var myId = id[0];
        var node = {
            id: myId,
            name: n[""name""],
            shortname: n[""shortname""],
            FullEntityName: n[""FullEntityName""],
            PCEID: n[""PCEID""],
            level: level,
            score: n[""score""],
            staleObjectsScore: n[""staleObjectsScore""],
            privilegiedGroupScore: n[""privilegiedGroupScore""],
            trustScore: n[""trustScore""],
            anomalyScore: n[""anomalyScore""],
            BU: n[""BU""],
            Entity: n[""Entity""]
        };
        nodes.push(node);
        if (parentId != 0) {
            var edge = {
                source: parentId,
                target: myId,
                rels: [parentName + ""->"" + n[""name""]]
            };
            edges.push(edge);
        }
        if ('children' in n) {
            for (var i = 0; i < n.children.length; i++) {
                var mydirection = direction;
                if (level == 0 && i > n.children.length / 2)
                    mydirection = -1;
                toNode(id, n.children[i], myId, n[""name""], level + mydirection, mydirection, nodes, edges);
            }
        }
    }
    toNode(id, tree, 0, """", 0, 1, nodes, edges);
    return { nodes: nodes, links: edges };
}

    function cartoSelectColor(n) {
        if (n['score'] <= 30) {
            return colors['lowscore'];
        }
        else if (n['score'] <= 50) {
            return colors['mediumscore'];
        }
        else if (n['score'] <= 70) {
            return colors['highscore'];
        }
        else if (n['score'] < 100) {
            return colors['superhighscore'];
        }
        else if (n['score'] == 100) {
            return colors['criticalscore'];
        }
        else
            return colors['unknown'];
    }

    function carto(data, hierachicalLayout) {
        var nodes = new vis.DataSet();
        var edges = new vis.DataSet();



        for (var i = 0; i < data.nodes.length; i++) {
            var n = data.nodes[i], node;

            node = {
                        // we use the count of the loop as an id if the id property setting is false
                        // this is in case the edges properties 'from' and 'to' are referencing
                        // the order of the node, not the real id.
                        id: n['id'],
                shortName: n['shortname'],
                value: 0 === n.dist ? 10 : 1,
                label: n['shortname'],
                title: getTooltipHtml(n),
                PCEID: n['PCEID'],
                level: n['level'],
                BU: n['BU'],
                Entity: n['Entity'],
                forest: n['forest'],
                color: cartoSelectColor(n)
            };
            nodes.add(node);
        }
        for (var j = 0; j < data.links.length; j++) {
            var l = data.links[j];
            var edge = {
                        from: l.source,
                to: l.target,
                data: {
                        rels: l.rels,
                    fromShortName: nodes.get(l.source).shortName,
                    fromBaseGroup: nodes.get(l.source).baseGroup,
                    toShortName: nodes.get(l.target).shortName,
                    toBaseGroup: nodes.get(l.target).baseGroup,
                    type: l.type
                },
                arrows: l.type === 'double' ? 'to, from' : 'to',
                title: l.rels.join('<br>'),
                color: {color: l.color, highlight: l.color, hover: l.color }
            };

            edges.add(edge);
        }

        // create a network
        var container = document.getElementById('mynetwork');
        var networkData = {
                            nodes: nodes,
            edges: edges
        };

        // create an array with nodes
        var options = {
                            //height: (window.innerHeight -130) + 'px',
                            //width: '100%',
                            height: '100%',
            autoResize: true,
            layout:
            {
                            improvedLayout: false
            },
            nodes: {
                            // you can use 'box', 'ellipse', 'circle', 'text' or 'database' here
                            // 'ellipse' is the default shape.
                            shape: 'ellipse',
                size: 20,
                font: {
                            //size: 15,
                            color: '#000000'
                    //face: 'arial' // maybe use a monospaced font?
                },
                borderWidth: 1,
                borderWidthSelected: 3,
                scaling: {
                            label: {
                            min: 15,
                        max: 25
                    }
                }
            },
            edges: {
                            width: 2,
                smooth: {
                            type: 'continuous'
                },
                hoverWidth: 2,
                selectionWidth: 2,
                arrows: {
                            to: {
                            enabled: true,
                        scaleFactor: 0.5
                    }, from: {
                            enabled: false,
                        scaleFactor: 0.5
                    }
                },
                color: {
                            //      inherit: 'from',
                            color: '#666666',
                    hover: '#333333',
                    highlight: '#000000'
                }
            },
            interaction: {
                            multiselect: true,
                hover: true,
                hideEdgesOnDrag: true
            }
        };
        if (hierachicalLayout) {
                            options.layout.hierarchical = { enabled: true, sortMethod: 'directed' };
                        } else {
                            options.physics = {
                                stabilization: {
                                    iterations: 2000 // try to stabilize the graph in 2000 times, after that show it anyway
                                },
                                barnesHut: {
                                    gravitationalConstant: -2000,
                                    centralGravity: 0.1,
                                    springLength: 95,
                                    springConstant: 0.04,
                                    damping: 0.09
                                },
                                enabled: true
                            };
                        }
        var network = new vis.Network(container, networkData, options);
        network.data = networkData;

        return network;
    }

var network;

var data = ");
			if (FullNodeMap)
			{
				Add(GenerateJsonFileFull(migrationChecker));
			}
			else
			{
				Add(GenerateJsonFileSimple(CenterDomainForSimpliedGraph));
				Add(@"; data = reshape(data)");
			}
			Add(@";

if (data.nodes.length > 0)
        $('#loadingModal').modal('show');
");
			Add(@"
network = carto(data,");
			if (FullNodeMap)
				Add("false");
			else
				Add("true");
			Add(@");
var progressBar = $('#loadingModal .progress-bar');

    network.on('stabilizationProgress', function (params) {
        var percentVal = 100 * params.iterations / params.total;
        progressBar.css('width', percentVal + '%').attr('aria-valuenow', percentVal + '%').text(percentVal + '%');
    });
    network.once('stabilizationIterationsDone', function () {
        var percentVal = 100;
        progressBar.css('width', percentVal + '%').attr('aria-valuenow', percentVal + '%').text(percentVal + '%');
        // really clean the dom element
        setTimeout(function () {
            $('#loadingModal').modal('hide')
        }, 100);
    });

</script>
");
		}


        #region json file

        public string GenerateJsonFileFull(MigrationChecker migrationChecker)
        {
            Dictionary<int, int> idconversiontable = new Dictionary<int, int>();
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            // START OF NODES

            sb.Append("  \"nodes\": [");
            // it is important to put the root node as the first node for correct display
            int nodenumber = 0;
            bool firstnode = true;
            foreach (GraphNode node in Nodes)
            {
                if (!firstnode)
                {
                    sb.Append("    },");
                }
                else
                {
                    firstnode = false;
                }
                sb.Append("    {");
                sb.Append("      \"id\": " + nodenumber + ",");
                sb.Append("      \"shortname\": \"" + ReportHelper.EscapeJsonString(node.Domain.DomainName.Split('.')[0]) + "\"");
                if (node.IsPartOfARealForest())
                {
					sb.Append("      ,\"forest\": \"" + ReportHelper.EscapeJsonString(node.Forest.DomainName) + "\"");
                }
                var entity = node.Entity;
                if (entity != null)
                {
                    sb.Append(entity.GetJasonOutput());
                }
                HealthcheckData data = node.HealthCheckData;
				sb.Append("      ,\"name\": \"" + ReportHelper.EscapeJsonString(node.Domain.DomainName) + "\"");
                if (data != null)
                {
                    sb.Append("      ,\"score\": " + data.GlobalScore);
                    sb.Append("      ,\"staleObjectsScore\": " + data.StaleObjectsScore);
                    sb.Append("      ,\"privilegiedGroupScore\": " + data.PrivilegiedGroupScore);
                    sb.Append("      ,\"trustScore\": " + data.TrustScore);
                    sb.Append("      ,\"anomalyScore\": " + data.AnomalyScore);
                    if (data.UserAccountData != null)
                        sb.Append("      ,\"activeusers\": " + data.UserAccountData.NumberActive);
                    if (data.ComputerAccountData != null)
                        sb.Append("      ,\"activecomputers\": " + data.ComputerAccountData.NumberActive);
                }
                sb.Append("      ,\"dist\": null");
                idconversiontable[node.Id] = nodenumber++;
            }
            if (Nodes.Count > 0)
            {
                sb.Append("    }");
            }
            sb.Append("  ],");
            // END OF NODES
            // START LINKS
            sb.Append("  \"links\": [");
            // avoid a final ","
            bool absenceOfLinks = true;
            // subtility: try to regroup 2 links at one if all the properties match
            // SkipLink contains the edge to ignore
            List<GraphEdge> SkipLink = new List<GraphEdge>();
            // foreach edge
            foreach (GraphNode node in Nodes)
            {
                foreach (GraphEdge edge in node.Trusts.Values)
                {

                    if (SkipLink.Contains(edge))
                        continue;
                    // for unidirectional trusts
                    // keep only the remote part of the trust. SID Filtering is unknown (avoid evaluating SID Filtering when no value is available)
                    if (edge.TrustDirection == 2 && edge.IsAuthoritative == false)
                        continue;
                    // keep only the reception of the trust. SID Filtering status is sure
                    if (edge.TrustDirection == 1 && edge.Destination.Trusts[edge.Source.Domain].IsAuthoritative == true)
                        continue;
                    // trying to simplify bidirectional trusts
                    bool isBidirectional = false;
                    if (edge.IsEquivalentToReverseEdge(migrationChecker))
                    {
                        GraphEdge reverseEdge = edge.Destination.Trusts[edge.Source.Domain];
                        // keep only one of the two part of the bidirectional trust
                        SkipLink.Add(reverseEdge);
                        isBidirectional = true;
                    }
                    if (!absenceOfLinks)
                    {
                        sb.Append("    },");
                    }
                    else
                    {
                        absenceOfLinks = false;
                    }
                    sb.Append("    {");
                    if (edge.TrustDirection == 2)
                    {
                        sb.Append("      \"source\": " + idconversiontable[edge.Destination.Id] + ",");
                        sb.Append("      \"target\": " + idconversiontable[edge.Source.Id] + ",");
                    }
                    else
                    {
                        sb.Append("      \"source\": " + idconversiontable[edge.Source.Id] + ",");
                        sb.Append("      \"target\": " + idconversiontable[edge.Destination.Id] + ",");
                    }
                    // blue: 25AEE4
                    // orange: FA9426
                    string sidFiltering = edge.GetSIDFilteringStatus(migrationChecker);
                    if (!edge.IsActive)
                    {
                        // purple
                        sb.Append("      \"color\": \"#A856AA\",");
                    }
                    else 
                    {
                        switch (sidFiltering)
                        {
                            case "Remote":
                                // yellow
                                sb.Append("      \"color\": \"#FDC334\",");
                                break;
                            case "Migration":
                                // blue
                                sb.Append("      \"color\": \"#25AEE4\",");
                                break;
                            case "No":
                                // red
                                sb.Append("      \"color\": \"#E75351\",");
                                break;
                            case "Yes":
                                // green
                                sb.Append("      \"color\": \"#74C25C\",");
                                break;
                        }
                    }
                    if (isBidirectional)
                    {
                        sb.Append("      \"type\": \"double\",");
                    }
                    sb.Append("      \"rels\": [\"");
                    sb.Append("Attributes=" + edge.GetTrustAttributes() + ",");
                    if (edge.CreationDate != DateTime.MinValue)
                    {
                        sb.Append("CreationDate=" + edge.CreationDate.ToString("yyyy-MM-dd") + ",");
                    }
                    sb.Append("SIDFiltering=" + sidFiltering);
                    sb.Append((edge.IsActive ? null : ",Inactive"));
                    sb.Append("\"]");

                }
            }
            if (!absenceOfLinks)
            {
                sb.Append("    }");
            }
            sb.Append("  ]");
            // END OF LINKS
            sb.Append("}");
            return sb.ToString();
        }

		public string GenerateJsonFileSimple(string domainToCenter)
		{
			int coveredNodesCount;
			return GenerateJsonFileSimple(domainToCenter, out coveredNodesCount);
		}

        private string GenerateJsonFileSimple(string domainToCenter,
                                            out int coveredNodesCount)
        {
            GraphNode center = null;
            StringBuilder sb = new StringBuilder();
            if (String.IsNullOrEmpty(domainToCenter))
            {
                Trace.WriteLine("finding the center domain");
                // find the domain with the most links
                int max = 0;
                foreach (var nodeToInvestigate in Nodes)
                {
                    if (nodeToInvestigate.Trusts.Count > max)
                    {
                        max = nodeToInvestigate.Trusts.Count;
                        center = nodeToInvestigate;
                    }
                }
                if (center == null)
                {
                    string output = null;
                    Trace.WriteLine("no domain found");
                    sb.Append("{");
                    sb.Append("  \"name\": \"No domain found\"\r\n");
                    sb.Append("}");
                    coveredNodesCount = 0;
                    return output;
                }
                if (Log != null)
                {
                    Log.Invoke("Simplified graph: automatic center on " + center);
                    Log.Invoke("Simplified graph: you can change this with --center-on <domain>");
                }
            }
            else
            {
                center = Nodes.GetDomain(domainToCenter.ToLowerInvariant()); 
                if (center == null)
                {
                    string output = null;
                    Trace.WriteLine(domainToCenter + " not found");
                    sb.Append("{");
                    sb.Append("  \"name\": \"" + domainToCenter + "\"\r\n");
                    sb.Append("}");
                    if (Log != null)
                    {
                        Log.Invoke("Simplified graph: domain " + domainToCenter + " not found.");
                    }
                    coveredNodesCount = 1;
                    return output;
                }
            }
            GraphNode newCentralNode = GenerateSimplifiedGraph(Nodes, center);
            coveredNodesCount = CountSimplifiedNodes(newCentralNode); if (Log != null)
            if (Log != null)
            {
                Log.Invoke("Simplified graph: contains " + coveredNodesCount + " nodes on a total of " + Nodes.Count);
            }
            GenerateSimplifiedJason(sb,newCentralNode);
            return sb.ToString();
        }

        // make a clone of all GraphNode except that only a few GraphEdge are kept
        // remove all uneeded GraphEdge to have only one GraphEdge between 2 GraphNodes (direct or indirect link)
        private GraphNode GenerateSimplifiedGraph(GraphNodeCollection nodes, GraphNode centralNode)
        {
            List<GraphNode> nodeAlreadyExamined = new List<GraphNode>();

            GraphNode output = GraphNode.CloneWithoutTrusts(centralNode);

            Dictionary<DomainKey, GraphNode> graph = new Dictionary<DomainKey, GraphNode>();
            graph.Add(output.Domain, output);

            List<GraphNode> nodesToExamine = new List<GraphNode>();
            nodesToExamine.Add(centralNode);
            // proceed layer by layer
            for (int currentLevel = 0; ; currentLevel++)
            {
                List<GraphNode> nodesToExamineForNextLevel = new List<GraphNode>();
                // this first iteration is important
                // it avoid a recursing exploration
                foreach (GraphNode nodeToExamine in nodesToExamine)
                {
                    nodeAlreadyExamined.Add(nodeToExamine);
                }
                foreach (GraphNode nodeToExamine in nodesToExamine)
                {
                    foreach (GraphEdge edge in nodeToExamine.Trusts.Values)
                    {
                        if (!nodeAlreadyExamined.Contains(edge.Destination)
                            && !nodesToExamine.Contains(edge.Destination)
                            && !nodesToExamineForNextLevel.Contains(edge.Destination))
                        {
                            // make a clone and add one GraphEdge
                            nodesToExamineForNextLevel.Add(edge.Destination);
                            graph.Add(edge.Destination.Domain, GraphNode.CloneWithoutTrusts(edge.Destination));
                            GraphEdge newEdge = new GraphEdge(graph[nodeToExamine.Domain], graph[edge.Destination.Domain], null, false);
                            graph[nodeToExamine.Domain].Trusts.Add(edge.Destination.Domain, newEdge);
                        }
                    }
                }
                if (nodesToExamineForNextLevel.Count == 0)
                    break;
                nodesToExamine = nodesToExamineForNextLevel;
            }
            return output;
        }

        private int CountSimplifiedNodes(GraphNode centralNode)
        {
            int num = 1;
            foreach (GraphEdge edge in centralNode.Trusts.Values)
            {
                num += CountSimplifiedNodes(edge.Destination);
            }
            return num;
        }

        private void GenerateSimplifiedJason(StringBuilder sb, GraphNode node)
        {
            sb.Append("{");
			sb.Append("  \"name\": \"" + ReportHelper.EscapeJsonString(node.Domain.DomainName) + "\"\r\n");
			sb.Append("  ,\"shortname\": \"" + ReportHelper.EscapeJsonString(node.Domain.DomainName.Split('.')[0]) + "\"\r\n");
            if (node.Forest != null && node.Forest != node.Domain)
            {
				sb.Append("      ,\"forest\": \"" + ReportHelper.EscapeJsonString(node.Forest.DomainName) + "\"");
            }
            HealthcheckData data = node.HealthCheckData;
            if (data != null)
            {
                sb.Append("      ,\"score\": " + data.GlobalScore);
                sb.Append("      ,\"staleObjectsScore\": " + data.StaleObjectsScore);
                sb.Append("      ,\"privilegiedGroupScore\": " + data.PrivilegiedGroupScore);
                sb.Append("      ,\"trustScore\": " + data.TrustScore);
                sb.Append("      ,\"anomalyScore\": " + data.AnomalyScore);
            }
            var entity = node.Entity;
            if (entity != null)
            {
                sb.Append(entity.GetJasonOutput());
            }
            if (node.Trusts.Count > 0)
            {
                sb.Append("      ,\"children\": [\r\n");
                int numChildren = 0;
                foreach (GraphEdge edge in node.Trusts.Values)
                {
                    if (numChildren != 0)
                    {
                        sb.Append(",\r\n");
                    }
                    GenerateSimplifiedJason(sb, edge.Destination);
                    numChildren++;
                }
                sb.Append("      ]\r\n");
            }
            sb.Append("}");
        }


		public string GenerateJsonFileChordDiagram(MigrationChecker migrationChecker)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			bool firstnode = true;
			foreach (GraphNode node in Nodes)
			{
				if (!firstnode)
				{
					sb.AppendLine(",");
				}
				else
				{
					firstnode = false;
				}
				sb.Append("    {");
				sb.Append("      \"name\": \"" + ReportHelper.EscapeJsonString(node.Domain.DomainName) + "\"");
				var entity = node.Entity;
				if (entity != null)
				{
					sb.Append(entity.GetJasonOutput());
				}
				HealthcheckData data = node.HealthCheckData;
				if (data != null)
				{
					sb.Append("      ,\"score\": " + data.GlobalScore);
					sb.Append("      ,\"staleObjectsScore\": " + data.StaleObjectsScore);
					sb.Append("      ,\"privilegiedGroupScore\": " + data.PrivilegiedGroupScore);
					sb.Append("      ,\"trustScore\": " + data.TrustScore);
					sb.Append("      ,\"anomalyScore\": " + data.AnomalyScore);
					if (data.UserAccountData != null)
						sb.Append("      ,\"activeusers\": " + data.UserAccountData.NumberActive);
					if (data.ComputerAccountData != null)
						sb.Append("      ,\"activecomputers\": " + data.ComputerAccountData.NumberActive);
				}
				sb.Append("      ,\"trusts\": [");
				bool firstTrust = true;
				foreach (var edge in node.Trusts.Values)
				{
					var destination = edge.Destination;
					if (!firstTrust)
					{
						sb.Append(",");
					}
					else
					{
						firstTrust = false;
					}
					sb.Append("    {");
					sb.Append("\"name\": \"");
					sb.Append(ReportHelper.EscapeJsonString(destination.Domain.DomainName));
					sb.Append("\"");
					var entity2 = destination.Entity;
					if (entity2 != null)
					{
						sb.Append(entity2.GetJasonOutput());
					}
					sb.Append("}");
				}
				sb.Append("]");
				sb.Append("}");

			}
			
			sb.AppendLine("]");
			return sb.ToString();
		}

		#endregion json file
	}
}
