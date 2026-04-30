import React from "react";
import { ActivityIndicator, Text, TextInput, TouchableOpacity, View } from "react-native";

interface AddressSearchInputProps {
  value: string;
  onChangeText: (text: string) => void;
  onSubmit: () => void;
  isLoading: boolean;
  placeholder?: string;
  submitLabel?: string;
  error?: string | null;
}

const AddressSearchInput: React.FC<AddressSearchInputProps> = ({
  value,
  onChangeText,
  onSubmit,
  isLoading,
  placeholder = "E.g. 29 Scone Street, Cranbourne",
  submitLabel = "Continue",
  error,
}) => {
  return (
    <View className="gap-y-3">
      <TextInput
        className="border border-gray-300 rounded-lg px-4 py-3 text-base text-black bg-white"
        placeholder={placeholder}
        placeholderTextColor="#9CA3AF"
        value={value}
        onChangeText={onChangeText}
        autoCapitalize="words"
        autoCorrect={false}
        returnKeyType="done"
        onSubmitEditing={onSubmit}
        editable={!isLoading}
      />

      {error ? (
        <Text className="text-red-500 text-sm">{error}</Text>
      ) : null}

      {isLoading ? (
        <ActivityIndicator size="small" color="#000000" />
      ) : (
        <TouchableOpacity
          className="bg-black rounded-lg py-4 items-center"
          onPress={onSubmit}
          disabled={!value.trim()}
          activeOpacity={0.8}
        >
          <Text className="text-white font-semibold text-base">{submitLabel}</Text>
        </TouchableOpacity>
      )}
    </View>
  );
};

export default AddressSearchInput;