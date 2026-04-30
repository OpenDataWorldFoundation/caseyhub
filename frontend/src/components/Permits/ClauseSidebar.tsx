import { ClauseDto } from "@/src/types/permitchecker/responses";
import React from "react";
import { ScrollView, Text, View } from "react-native";
import ClauseItem from "./ClauseItem";

interface ClauseSidebarProps {
  clauses: ClauseDto[];
}

const ClauseSidebar: React.FC<ClauseSidebarProps> = ({ clauses }) => {
  if (clauses.length === 0) return null;

  return (
    <View className="mt-6 border-t border-gray-100 pt-4">
      <Text className="text-xs font-semibold text-gray-400 uppercase tracking-widest mb-3">
        Relevant Clauses
      </Text>
      <ScrollView nestedScrollEnabled showsVerticalScrollIndicator={false}>
        {clauses.map((clause) => (
          <ClauseItem key={clause.clauseNumber} clause={clause} />
        ))}
      </ScrollView>
    </View>
  );
};

export default ClauseSidebar;