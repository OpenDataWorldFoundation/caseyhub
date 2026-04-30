import { EvaluationOutcome } from "@/src/types/permitchecker/responses";
import React from "react";
import { Text, View } from "react-native";

interface VerdictCardProps {
  outcome: EvaluationOutcome;
  summary: string;
}

const OUTCOME_CONFIG: Record<
  EvaluationOutcome,
  { label: string; borderClass: string; labelClass: string; bgClass: string }
> = {
  PermitRequired: {
    label: "Planning Permit Required",
    borderClass: "border-red-500",
    labelClass: "text-red-600",
    bgClass: "bg-red-50",
  },
  NoPermitRequired: {
    label: "No Planning Permit Required",
    borderClass: "border-green-500",
    labelClass: "text-green-700",
    bgClass: "bg-green-50",
  },
  Exempt: {
    label: "Exempt",
    borderClass: "border-green-500",
    labelClass: "text-green-700",
    bgClass: "bg-green-50",
  },
  ReferToCouncil: {
    label: "Refer to Casey Council",
    borderClass: "border-amber-500",
    labelClass: "text-amber-700",
    bgClass: "bg-amber-50",
  },
};

const VerdictCard: React.FC<VerdictCardProps> = ({ outcome, summary }) => {
  const config = OUTCOME_CONFIG[outcome];

  return (
    <View className={`border-l-4 rounded-lg p-4 mb-6 ${config.borderClass} ${config.bgClass}`}>
      <Text className={`text-base font-bold mb-2 ${config.labelClass}`}>{config.label}</Text>
      <Text className="text-sm text-gray-700 leading-relaxed">{summary}</Text>
    </View>
  );
};

export default VerdictCard;