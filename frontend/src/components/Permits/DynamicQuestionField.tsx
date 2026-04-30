import { QuestionDto } from "@/src/types/permitchecker/responses";
import React from "react";
import { Text, TextInput, TouchableOpacity, View } from "react-native";

interface DynamicQuestionFieldProps {
  question: QuestionDto;
  value: unknown;
  onAnswer: (fieldKey: string, value: unknown) => void;
}

const DynamicQuestionField: React.FC<DynamicQuestionFieldProps> = ({
  question,
  value,
  onAnswer,
}) => {
  const handleChange = (newValue: unknown) => {
    onAnswer(question.fieldKey, newValue);
  };

  return (
    <View className="mb-6">
      <Text className="text-base font-medium text-black mb-1">{question.questionText}</Text>

      {question.helpText ? (
        <Text className="text-sm text-gray-500 mb-2">{question.helpText}</Text>
      ) : null}

      {question.inputType === "Number" && (
        <NumberInput
          value={value as string}
          onChange={handleChange}
          validation={question.validation}
        />
      )}

      {question.inputType === "SingleSelect" && question.options && (
        <SingleSelectInput
          options={question.options}
          value={value as string}
          onChange={handleChange}
        />
      )}

      {question.inputType === "Boolean" && (
        <BooleanInput value={value as boolean | undefined} onChange={handleChange} />
      )}

      {question.inputType === "MultiSelect" && question.options && (
        <MultiSelectInput
          options={question.options}
          value={(value as string[]) ?? []}
          onChange={handleChange}
        />
      )}
    </View>
  );
};

// ── Sub-renderers ─────────────────────────────────────────────────────────────

interface NumberInputProps {
  value: string;
  onChange: (value: number | string) => void;
  validation: QuestionDto["validation"];
}

const NumberInput: React.FC<NumberInputProps> = ({ value, onChange, validation }) => {
  const unit = validation?.unit ? ` (${validation.unit})` : "";
  return (
    <View>
      <TextInput
        className="border border-gray-300 rounded-lg px-4 py-3 text-base text-black bg-white"
        placeholder={`Enter value${unit}`}
        placeholderTextColor="#9CA3AF"
        value={value != null ? String(value) : ""}
        onChangeText={(text) => {
          // Allow intermediate decimal input (e.g. "1.")
          if (text === "" || text === ".") {
            onChange(text);
            return;
          }
          const parsed = parseFloat(text);
          if (!isNaN(parsed)) {
            onChange(parsed);
          }
        }}
        keyboardType="decimal-pad"
      />
      {validation && (validation.min != null || validation.max != null) ? (
        <Text className="text-xs text-gray-400 mt-1">
          {validation.min != null ? `Min: ${validation.min}` : ""}
          {validation.min != null && validation.max != null ? " — " : ""}
          {validation.max != null ? `Max: ${validation.max}` : ""}
          {unit}
        </Text>
      ) : null}
    </View>
  );
};

interface SingleSelectInputProps {
  options: { value: string; label: string }[];
  value: string;
  onChange: (value: string) => void;
}

const SingleSelectInput: React.FC<SingleSelectInputProps> = ({ options, value, onChange }) => {
  return (
    <View className="gap-y-2">
      {options.map((option) => {
        const isSelected = value === option.value;
        return (
          <TouchableOpacity
            key={option.value}
            className={`border rounded-lg px-4 py-3 flex-row items-center justify-between ${
              isSelected ? "border-black bg-black" : "border-gray-300 bg-white"
            }`}
            onPress={() => onChange(option.value)}
            activeOpacity={0.7}
          >
            <Text className={`text-sm font-medium ${isSelected ? "text-white" : "text-black"}`}>
              {option.label}
            </Text>
            {isSelected ? <Text className="text-white text-xs">✓</Text> : null}
          </TouchableOpacity>
        );
      })}
    </View>
  );
};

interface BooleanInputProps {
  value: boolean | undefined;
  onChange: (value: boolean) => void;
}

const BooleanInput: React.FC<BooleanInputProps> = ({ value, onChange }) => {
  const options = [
    { label: "Yes", value: true },
    { label: "No", value: false },
  ];

  return (
    <View className="flex-row gap-x-3">
      {options.map((option) => {
        const isSelected = value === option.value;
        return (
          <TouchableOpacity
            key={String(option.value)}
            className={`flex-1 border rounded-lg py-3 items-center ${
              isSelected ? "border-black bg-black" : "border-gray-300 bg-white"
            }`}
            onPress={() => onChange(option.value)}
            activeOpacity={0.7}
          >
            <Text className={`font-medium text-sm ${isSelected ? "text-white" : "text-black"}`}>
              {option.label}
            </Text>
          </TouchableOpacity>
        );
      })}
    </View>
  );
};

interface MultiSelectInputProps {
  options: { value: string; label: string }[];
  value: string[];
  onChange: (value: string[]) => void;
}

const MultiSelectInput: React.FC<MultiSelectInputProps> = ({ options, value, onChange }) => {
  const toggle = (optionValue: string) => {
    const updated = value.includes(optionValue)
      ? value.filter((v) => v !== optionValue)
      : [...value, optionValue];
    onChange(updated);
  };

  return (
    <View className="gap-y-2">
      {options.map((option) => {
        const isSelected = value.includes(option.value);
        return (
          <TouchableOpacity
            key={option.value}
            className={`border rounded-lg px-4 py-3 flex-row items-center justify-between ${
              isSelected ? "border-black bg-black" : "border-gray-300 bg-white"
            }`}
            onPress={() => toggle(option.value)}
            activeOpacity={0.7}
          >
            <Text className={`text-sm font-medium ${isSelected ? "text-white" : "text-black"}`}>
              {option.label}
            </Text>
            {isSelected ? <Text className="text-white text-xs">✓</Text> : null}
          </TouchableOpacity>
        );
      })}
    </View>
  );
};

export default DynamicQuestionField;