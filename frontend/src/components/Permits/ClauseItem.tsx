import { ClauseDto } from "@/src/types/permitchecker/responses";
import React, { useState } from "react";
import { Linking, Text, TouchableOpacity, View } from "react-native";

interface ClauseItemProps {
  clause: ClauseDto;
}

const ClauseItem: React.FC<ClauseItemProps> = ({ clause }) => {
  const [expanded, setExpanded] = useState(false);

  const handleOpenUrl = () => {
    if (clause.officialUrl) {
      Linking.openURL(clause.officialUrl);
    }
  };

  return (
    <TouchableOpacity
      className="py-3 border-b border-gray-100"
      onPress={() => setExpanded((prev) => !prev)}
      activeOpacity={0.7}
    >
      <View className="flex-row items-center justify-between">
        <Text className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
          Clause {clause.clauseNumber}
        </Text>
        <Text className="text-xs text-gray-400">{expanded ? "▲" : "▼"}</Text>
      </View>

      <Text className="text-sm font-medium text-black mt-0.5">{clause.title}</Text>

      {expanded && clause.summary ? (
        <Text className="text-xs text-gray-600 mt-2 leading-relaxed">{clause.summary}</Text>
      ) : null}

      {expanded && clause.officialUrl ? (
        <TouchableOpacity onPress={handleOpenUrl} className="mt-2">
          <Text className="text-xs text-blue-600 underline">View official clause →</Text>
        </TouchableOpacity>
      ) : null}
    </TouchableOpacity>
  );
};

export default ClauseItem;