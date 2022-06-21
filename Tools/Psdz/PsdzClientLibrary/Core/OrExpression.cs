﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	[Serializable]
	public class OrExpression : RuleExpression
	{
		public OrExpression()
		{
			this.operands = new RuleExpression[0];
		}

		public OrExpression(RuleExpression firstOperand, RuleExpression secondOperand)
		{
			this.operands = new RuleExpression[2];
			this.operands[0] = firstOperand;
			this.operands[1] = secondOperand;
		}

		public int Length
		{
			get
			{
				return this.operands.Length;
			}
		}

		public RuleExpression this[int index]
		{
			get
			{
				return this.operands[index];
			}
			set
			{
				this.operands[index] = value;
			}
		}

		public new static OrExpression Deserialize(Stream ms, Vehicle vec)
		{
			byte[] bytes = BitConverter.GetBytes(0);
			ms.Read(bytes, 0, bytes.Length);
			int num = BitConverter.ToInt32(bytes, 0);
			OrExpression orExpression = new OrExpression();
			for (int i = 0; i < num; i++)
			{
				orExpression.AddOperand(RuleExpression.Deserialize(ms, vec));
			}
			return orExpression;
		}

		public void AddOperand(RuleExpression operand)
		{
			RuleExpression[] array = new RuleExpression[this.operands.Length + 1];
			Array.Copy(this.operands, array, this.operands.Length);
			array[array.Length - 1] = operand;
			this.operands = array;
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			bool flag = false;
			internalResult.RuleExpression = this;
			foreach (RuleExpression ruleExpression in this.operands)
			{
				flag |= RuleExpression.Evaluate(vec, ruleExpression, ffmResolver, internalResult);
			}
			return flag;
		}

		public override EEvaluationResult EvaluateEmpiricalRule(long[] premises)
		{
			RuleExpression[] array = this.operands;
			for (int i = 0; i < array.Length; i++)
			{
				EEvaluationResult eevaluationResult = array[i].EvaluateEmpiricalRule(premises);
				if (eevaluationResult != EEvaluationResult.INVALID)
				{
					return eevaluationResult;
				}
			}
			return EEvaluationResult.INVALID;
		}

		public override EEvaluationResult EvaluateFaultClassRule(Dictionary<string, List<double>> variables)
		{
			RuleExpression[] array = this.operands;
			for (int i = 0; i < array.Length; i++)
			{
				EEvaluationResult eevaluationResult = array[i].EvaluateFaultClassRule(variables);
				if (eevaluationResult != EEvaluationResult.INVALID)
				{
					return eevaluationResult;
				}
			}
			return EEvaluationResult.INVALID;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			bool flag = false;
			this.missingCharacteristics.Clear();
			this.missingVariants.Clear();
			EEvaluationResult result = EEvaluationResult.INVALID;
			foreach (RuleExpression ruleExpression in this.operands)
			{
				EEvaluationResult eevaluationResult = ruleExpression.EvaluateVariantRule(client, baseConfiguration, ecus);
				switch (eevaluationResult)
				{
					case EEvaluationResult.VALID:
						this.missingCharacteristics.Clear();
						this.missingVariants.Clear();
						return eevaluationResult;
					case EEvaluationResult.INVALID:
						break;
					case EEvaluationResult.MISSING_CHARACTERISTIC:
						this.missingCharacteristics.AddRange(ruleExpression.GetUnknownCharacteristics(baseConfiguration));
						if (!flag)
						{
							flag = true;
							result = eevaluationResult;
						}
						break;
					case EEvaluationResult.MISSING_VARIANT:
						this.missingVariants.AddRange(ruleExpression.GetUnknownVariantIds(ecus));
						if (!flag)
						{
							flag = true;
							result = eevaluationResult;
						}
						break;
					default:
						throw new Exception("Unknown result");
				}
			}
			return result;
		}

		public override long GetExpressionCount()
		{
			long num = 1L;
			foreach (RuleExpression ruleExpression in this.operands)
			{
				num += ruleExpression.GetExpressionCount();
			}
			return num;
		}

		public override long GetMemorySize()
		{
			long num = (long)this.operands.Length * 8L + 8L;
			foreach (RuleExpression ruleExpression in this.operands)
			{
				num += ruleExpression.GetMemorySize();
			}
			return num;
		}

		public override IList<long> GetUnknownCharacteristics(CharacteristicSet baseConfiguration)
		{
			return this.missingCharacteristics;
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			return this.missingVariants;
		}

		public override void Optimize()
		{
			List<RuleExpression> list = new List<RuleExpression>();
			foreach (RuleExpression ruleExpression in this.operands)
			{
				ruleExpression.Optimize();
				if (ruleExpression is AndExpression)
				{
					AndExpression andExpression = (AndExpression)ruleExpression;
					if (andExpression.Length == 1)
					{
						list.Add(andExpression[0]);
					}
					else
					{
						list.Add(andExpression);
					}
				}
				else if (ruleExpression is OrExpression)
				{
					OrExpression orExpression = (OrExpression)ruleExpression;
					if (orExpression.operands.Length != 0)
					{
						list.AddRange(orExpression.operands);
					}
				}
				else
				{
					list.Add(ruleExpression);
				}
			}
			this.operands = list.ToArray();
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(2);
			byte[] bytes = BitConverter.GetBytes(this.operands.Length);
			ms.Write(bytes, 0, bytes.Length);
			RuleExpression[] array = this.operands;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Serialize(ms);
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			for (int i = 0; i < this.operands.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(" OR ");
				}
				stringBuilder.Append(this.operands[i]);
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

        public string ToFormula()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            for (int i = 0; i < this.operands.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(" || ");
                }
                stringBuilder.Append(this.operands[i]);
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private readonly List<long> missingCharacteristics = new List<long>();

		private readonly List<long> missingVariants = new List<long>();

		private RuleExpression[] operands;
	}
}
