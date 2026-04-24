import React, { useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRouter } from "expo-router";
import {
  ArrowRight,
  LogOut,
  Mail,
  ShieldCheck,
  UserRound,
} from "lucide-react-native";

import { AuthApiError } from "@/src/features/auth/auth.api";
import {
  LoginRequestDto,
  RegisterRequestDto,
} from "@/src/features/auth/types";
import { useAuth } from "@/src/providers/AuthProvider";

type AuthMode = "login" | "signup";

interface AuthFormState {
  name: string;
  email: string;
  password: string;
}

const initialFormState: AuthFormState = {
  name: "",
  email: "",
  password: "",
};

const validateForm = (mode: AuthMode, form: AuthFormState) => {
  const errors: Partial<Record<keyof AuthFormState, string>> = {};

  if (mode === "signup" && !form.name.trim()) {
    errors.name = "Name is required.";
  }

  if (!form.email.trim()) {
    errors.email = "Email is required.";
  } else if (!/\S+@\S+\.\S+/.test(form.email.trim())) {
    errors.email = "Enter a valid email address.";
  }

  if (!form.password) {
    errors.password = "Password is required.";
  } else if (form.password.length < 8) {
    errors.password = "Password must be at least 8 characters.";
  }

  return errors;
};

const ProfilePage = () => {
  const router = useRouter();
  const { isAuthenticated, login, logout, register, status, user } = useAuth();
  const [mode, setMode] = useState<AuthMode>("login");
  const [form, setForm] = useState<AuthFormState>(initialFormState);
  const [fieldErrors, setFieldErrors] = useState<
    Partial<Record<keyof AuthFormState, string>>
  >({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const isBusy = isSubmitting || isLoggingOut;
  const heading =
    mode === "login"
      ? "Pick up where you left off"
      : "Create your Casey account";

  const resetErrors = () => {
    setFieldErrors({});
    setSubmitError(null);
  };

  const handleModeChange = (nextMode: AuthMode) => {
    setMode(nextMode);
    setForm(initialFormState);
    resetErrors();
  };

  const handleFieldChange = (
    field: keyof AuthFormState,
    value: string,
  ) => {
    setForm((currentForm) => ({
      ...currentForm,
      [field]: value,
    }));

    setFieldErrors((currentErrors) => ({
      ...currentErrors,
      [field]: undefined,
    }));
  };

  const handleSubmit = async () => {
    const errors = validateForm(mode, form);

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      return;
    }

    resetErrors();
    setIsSubmitting(true);

    try {
      if (mode === "login") {
        const payload: LoginRequestDto = {
          email: form.email.trim(),
          password: form.password,
        };

        await login(payload);
      } else {
        const payload: RegisterRequestDto = {
          name: form.name.trim(),
          email: form.email.trim(),
          password: form.password,
        };

        await register(payload);
      }

      router.replace("/");
    } catch (error) {
      console.log("error: " + error);
      const message =
        error instanceof AuthApiError
          ? error.message
          : "We couldn't complete that request. Please try again.";

      setSubmitError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleLogout = async () => {
    setSubmitError(null);
    setIsLoggingOut(true);

    try {
      await logout();
    } finally {
      setIsLoggingOut(false);
    }
  };

  if (status === "loading") {
    return (
      <SafeAreaView className="flex-1 bg-stone-950">
        <View className="flex-1 items-center justify-center gap-y-4 px-6">
          <ActivityIndicator color="#F4F4F5" size="small" />
          <Text className="text-sm font-medium text-zinc-300">
            Restoring your session...
          </Text>
        </View>
      </SafeAreaView>
    );
  }

  if (isAuthenticated && user) {
    return (
      <SafeAreaView className="flex-1 bg-stone-950">
        <ScrollView
          className="flex-1"
          contentContainerStyle={{ padding: 24, paddingBottom: 140 }}
          showsVerticalScrollIndicator={false}
        >
          <View className="rounded-[32px] bg-emerald-400 px-6 py-8">
            <Text className="text-xs font-semibold uppercase tracking-[2px] text-emerald-950">
              Profile
            </Text>
            <Text className="mt-4 text-4xl font-bold text-emerald-950">
              Welcome back, {user.name.split(" ")[0]}
            </Text>
            <Text className="mt-3 text-sm leading-6 text-emerald-950/80">
              Your CaseyHub account is active on this device. You can keep using
              the app and sign out here whenever you need to.
            </Text>
          </View>

          <View className="mt-5 rounded-[28px] border border-zinc-800 bg-zinc-900 px-5 py-6">
            <View className="flex-row items-center gap-x-3">
              <View className="h-12 w-12 items-center justify-center rounded-full bg-zinc-800">
                <UserRound color="#34D399" size={22} />
              </View>
              <View className="flex-1">
                <Text className="text-lg font-semibold text-zinc-50">
                  {user.name}
                </Text>
                <Text className="mt-1 text-sm text-zinc-400">
                  CaseyHub member
                </Text>
              </View>
            </View>

            <View className="mt-6 gap-y-4">
              <View className="rounded-2xl bg-zinc-950 px-4 py-4">
                <Text className="text-xs uppercase tracking-[2px] text-zinc-500">
                  Email
                </Text>
                <Text className="mt-2 text-base font-medium text-zinc-100">
                  {user.email}
                </Text>
              </View>

              <View className="rounded-2xl bg-zinc-950 px-4 py-4">
                <Text className="text-xs uppercase tracking-[2px] text-zinc-500">
                  Session
                </Text>
                <View className="mt-2 flex-row items-center gap-x-2">
                  <ShieldCheck color="#34D399" size={18} />
                  <Text className="text-base font-medium text-zinc-100">
                    Signed in securely
                  </Text>
                </View>
              </View>
            </View>

            <Pressable
              onPress={handleLogout}
              disabled={isBusy}
              className="mt-6 flex-row items-center justify-center rounded-full bg-zinc-50 px-5 py-4"
            >
              {isLoggingOut ? (
                <ActivityIndicator color="#18181B" size="small" />
              ) : (
                <>
                  <LogOut color="#18181B" size={18} />
                  <Text className="ml-2 text-sm font-semibold text-zinc-900">
                    Log out
                  </Text>
                </>
              )}
            </Pressable>
          </View>
        </ScrollView>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView className="flex-1 bg-stone-950">
      <ScrollView
        className="flex-1"
        contentContainerStyle={{ padding: 24, paddingBottom: 140 }}
        keyboardShouldPersistTaps="handled"
        showsVerticalScrollIndicator={false}
      >
       

        <View className="mt-5 rounded-[28px] border border-zinc-800 bg-zinc-900 px-5 py-6">
          <View className="flex-row rounded-full bg-zinc-950 p-1">
            {(["login", "signup"] as AuthMode[]).map((tab) => {
              const isActive = tab === mode;

              return (
                <Pressable
                  key={tab}
                  onPress={() => handleModeChange(tab)}
                  className={`flex-1 rounded-full px-4 py-3 ${
                    isActive ? "bg-zinc-100" : "bg-transparent"
                  }`}
                >
                  <Text
                    className={`text-center text-sm font-semibold ${
                      isActive ? "text-zinc-950" : "text-zinc-400"
                    }`}
                  >
                    {tab === "login" ? "Login" : "Sign up"}
                  </Text>
                </Pressable>
              );
            })}
          </View>

          <View className="mt-6">
            <Text className="text-2xl font-semibold text-zinc-50">
              {heading}
            </Text>
            <Text className="mt-2 text-sm leading-6 text-zinc-400">
              {mode === "login"
                ? "Use the email and password you registered with."
                : "A secure account helps you come back without starting over."}
            </Text>
          </View>

          <View className="mt-6 gap-y-4">
            {mode === "signup" ? (
              <View>
                <Text className="mb-2 text-sm font-medium text-zinc-300">
                  Full name
                </Text>
                <TextInput
                  value={form.name}
                  onChangeText={(value) => handleFieldChange("name", value)}
                  editable={!isBusy}
                  placeholder="John Doe"
                  placeholderTextColor="#71717A"
                  className="rounded-2xl border border-zinc-800 bg-zinc-950 px-4 py-4 text-base text-zinc-50"
                />
                {fieldErrors.name ? (
                  <Text className="mt-2 text-sm text-rose-400">
                    {fieldErrors.name}
                  </Text>
                ) : null}
              </View>
            ) : null}

            <View>
              <Text className="mb-2 text-sm font-medium text-zinc-300">
                Email
              </Text>
              <View className="rounded-2xl border border-zinc-800 bg-zinc-950 px-4">
                <View className="flex-row items-center">
                  <Mail color="#A1A1AA" size={18} />
                  <TextInput
                    value={form.email}
                    onChangeText={(value) => handleFieldChange("email", value)}
                    editable={!isBusy}
                    placeholder="name@example.com"
                    placeholderTextColor="#71717A"
                    autoCapitalize="none"
                    keyboardType="email-address"
                    className="ml-3 flex-1 py-4 text-base text-zinc-50"
                  />
                </View>
              </View>
              {fieldErrors.email ? (
                <Text className="mt-2 text-sm text-rose-400">
                  {fieldErrors.email}
                </Text>
              ) : null}
            </View>

            <View>
              <Text className="mb-2 text-sm font-medium text-zinc-300">
                Password
              </Text>
              <TextInput
                value={form.password}
                onChangeText={(value) => handleFieldChange("password", value)}
                editable={!isBusy}
                placeholder="Minimum 8 characters"
                placeholderTextColor="#71717A"
                secureTextEntry
                className="rounded-2xl border border-zinc-800 bg-zinc-950 px-4 py-4 text-base text-zinc-50"
              />
              {fieldErrors.password ? (
                <Text className="mt-2 text-sm text-rose-400">
                  {fieldErrors.password}
                </Text>
              ) : null}
            </View>
          </View>

          {submitError ? (
            <View className="mt-4 rounded-2xl border border-rose-500/30 bg-rose-500/10 px-4 py-3">
              <Text className="text-sm text-rose-300">{submitError}</Text>
            </View>
          ) : null}

          <Pressable
            onPress={handleSubmit}
            disabled={isBusy}
            className="mt-6 flex-row items-center justify-center rounded-full bg-amber-300 px-5 py-4"
          >
            {isSubmitting ? (
              <ActivityIndicator color="#422006" size="small" />
            ) : (
              <>
                <Text className="text-sm font-semibold text-amber-950">
                  {mode === "login" ? "Continue to Home" : "Create account"}
                </Text>
                <ArrowRight color="#422006" size={18} />
              </>
            )}
          </Pressable>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
};

export default ProfilePage;
