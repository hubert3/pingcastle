﻿//
// Copyright (c) Ping Castle. All rights reserved.
// https://www.pingcastle.com
//
// Licensed under the Non-Profit OSL. See LICENSE file in the project root for full license information.
//
using System;
using System.Collections.Generic;
using System.Text;
using PingCastle.Rules;

namespace PingCastle.Healthcheck.Rules
{
	[RuleModel("A-DC-Spooler", RiskRuleCategory.Anomalies, RiskModelCategory.PassTheCredential)]
	[RuleComputation(RuleComputationType.TriggerOnPresence, 10)]
	[RuleIntroducedIn(2, 6)]
	public class HeatlcheckRuleAnomalyDCSpooler : RuleBase<HealthcheckData>
    {
		protected override int? AnalyzeDataNew(HealthcheckData healthcheckData)
        {
			foreach (var DC in healthcheckData.DomainControllers)
			{
				if (DC.RemoteSpoolerDetected)
				{
					AddRawDetail(DC.DCName);
				}
			}
			return null;
        }
    }
}
