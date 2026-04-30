import { TriggeredRuleDto } from "@/src/types/permitchecker/responses";
import React from "react";
import { Linking, Text, TouchableOpacity, View } from "react-native";

interface TriggeredRuleItemProps {
  rule: TriggeredRuleDto;
}

const TriggeredRuleItem: React.FC<TriggeredRuleItemProps> = ({ rule }) => {
  const handleOpenClause = () => {
    if (rule.clause.officialUrl) {
      Linking.openURL(rule.clause.officialUrl);
    }
  };

  return (
    <View className="border border-gray-100 rounded-lg p-4 mb-3 bg-white">
      <Text className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">
        Clause {rule.clause.clauseNumber} — {rule.clause.title}
      </Text>
      <Text className="text-sm text-gray-800 leading-relaxed">{rule.outcomeReason}</Text>
      {rule.clause.officialUrl ? (
        <TouchableOpacity onPress={handleOpenClause} className="mt-2">
          <Text className="text-xs text-blue-600">View Clause {rule.clause.clauseNumber} →</Text>
        </TouchableOpacity>
      ) : null}
    </View>
  );
};

export default TriggeredRuleItem;