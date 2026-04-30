import { ClauseDto } from "@/src/types/permitchecker/responses";
import { router } from "expo-router";
import React from "react";
import { ScrollView, Text, TouchableOpacity, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import ClauseSidebar from "./ClauseSidebar";

interface WizardScreenWrapperProps {
  stepLabel: string;       // e.g. "Step 1 of 4"
  title: string;           // e.g. "What's your address?"
  subtitle?: string;       // optional subtitle
  clausesInScope: ClauseDto[];
  children: React.ReactNode;
  onBack?: () => void;     // if undefined, uses router.back()
  showBack?: boolean;      // default true
}

const WizardScreenWrapper = ({stepLabel,title,subtitle,clausesInScope,children,onBack,showBack = true}: WizardScreenWrapperProps) => {
   
  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      router.back();
    }
  };

  return (
    <SafeAreaView className="flex-1 bg-white">
      {/* <View className="bg-gray-900"> <Text className="text-center font-2xl font-bold text-white"> Permit Checker </Text> </View> */}
      <ScrollView
        className="flex-1"
        contentContainerStyle={{ padding: 24, paddingBottom: 48 }}
        showsVerticalScrollIndicator={false}
        keyboardShouldPersistTaps="handled"
      >
        {/* Header row */}
        <View className="flex-row items-center mb-6">
          {showBack && (
            <TouchableOpacity onPress={handleBack} className="mr-3 -ml-1 p-1">
              <Text className="text-base text-black">←</Text>
            </TouchableOpacity>
          )}
          <Text className="text-xs font-semibold text-gray-400 uppercase tracking-widest">
            {stepLabel}
          </Text>
        </View>

        {/* Title */}
        <Text className="text-2xl font-bold text-black mb-1">{title}</Text>

        {subtitle ? (
          <Text className="text-sm text-gray-500 mb-6">{subtitle}</Text>
        ) : (
          <View className="mb-6" />
        )}
        
        {/* All Passed Children*/}
        {children}

        {/* Relevant Clauses sidebar: visible once populated */}
        <ClauseSidebar clauses={clausesInScope} />

      </ScrollView>
    </SafeAreaView>
  );
};

export default WizardScreenWrapper;