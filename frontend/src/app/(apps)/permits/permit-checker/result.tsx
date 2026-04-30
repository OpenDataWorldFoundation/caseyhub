import TriggeredRuleItem from "@/src/components/Permits/TriggeredRuleItem";
import VerdictCard from "@/src/components/Permits/VerdictCard";
import WizardScreenWrapper from "@/src/components/Permits/WizardScreenWrapper";
import { usePermitCheckerStore } from "@/src/store/permitChckerStore";
import { router } from "expo-router";
import React from "react";
import { Text, TouchableOpacity, View } from "react-native";

export default function ResultScreen() {
  const outcome = usePermitCheckerStore((s) => s.outcome);
  const outcomeSummary = usePermitCheckerStore((s) => s.outcomeSummary);
  const triggeredRules = usePermitCheckerStore((s) => s.triggeredRules);
  const clausesInScope = usePermitCheckerStore((s) => s.clausesInScope);
  const normalisedAddress = usePermitCheckerStore((s) => s.normalisedAddress);
  const buildingTypeDisplayName = usePermitCheckerStore((s) => s.buildingTypeDisplayName);
  const zoneCode = usePermitCheckerStore((s) => s.zoneCode);
  const zoneDescription = usePermitCheckerStore((s) => s.zoneDescription);
  const reset = usePermitCheckerStore((s) => s.reset);

  const handleStartNew = () => {
    reset();
    router.replace("/(apps)/permits/permit-checker");
  };

  // Guard — should never be null here, but handle gracefully
  if (!outcome || !outcomeSummary) {
    return (
      <WizardScreenWrapper
        stepLabel="Result"
        title="Something went wrong"
        clausesInScope={[]}
      >
        <Text className="text-gray-500 text-sm">
          No assessment result found. Please start a new assessment.
        </Text>
        <TouchableOpacity
          className="bg-black rounded-lg py-4 items-center mt-6"
          onPress={handleStartNew}
        >
          <Text className="text-white font-semibold text-base">Start New Assessment</Text>
        </TouchableOpacity>
      </WizardScreenWrapper>
    );
  }

  return (
    <WizardScreenWrapper
      stepLabel="Assessment Complete"
      title="Your Result"
      clausesInScope={clausesInScope}
      showBack={false}
    >
      {/* Address + zone context */}
      <View className="mb-6 p-4 bg-gray-50 rounded-lg">
        <Text className="text-xs text-gray-500 mb-1">Address assessed</Text>
        <Text className="text-sm font-medium text-black">{normalisedAddress}</Text>
        {zoneCode ? (
          <Text className="text-xs text-gray-500 mt-1">
            Zone: {zoneCode}
            {zoneDescription ? ` — ${zoneDescription}` : ""}
          </Text>
        ) : null}
        {buildingTypeDisplayName ? (
          <Text className="text-xs text-gray-500">Project: {buildingTypeDisplayName}</Text>
        ) : null}
      </View>

      {/* Verdict */}
      <VerdictCard outcome={outcome} summary={outcomeSummary} />

      {/* Why — triggered rule breakdown */}
      {triggeredRules && triggeredRules.length > 0 && (
        <View className="mb-4">
          <Text className="text-xs font-semibold text-gray-400 uppercase tracking-widest mb-3">
            Why this result
          </Text>
          {triggeredRules.map((rule) => (
            <TriggeredRuleItem key={rule.ruleId} rule={rule} />
          ))}
        </View>
      )}

      {/* Disclaimer */}
      <View className="bg-gray-50 rounded-lg p-4 mb-6">
        <Text className="text-xs text-gray-500 leading-relaxed">
          This assessment is a guide only and is based on standard planning scheme provisions for
          the City of Casey. It does not constitute legal or planning advice. Always confirm
          requirements with Casey Council's Planning Department before commencing works.
        </Text>
      </View>

      {/* Actions */}
      <TouchableOpacity
        className="bg-black rounded-lg py-4 items-center mb-3"
        onPress={handleStartNew}
        activeOpacity={0.8}
      >
        <Text className="text-white font-semibold text-base">Start New Assessment</Text>
      </TouchableOpacity>

      <TouchableOpacity
        className="border border-gray-300 rounded-lg py-4 items-center"
        onPress={() => router.replace("/(apps)/permits")}
        activeOpacity={0.8}
      >
        <Text className="text-black font-medium text-base">Back to Permits</Text>
      </TouchableOpacity>
    </WizardScreenWrapper>
  );
}