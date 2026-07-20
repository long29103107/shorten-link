import { useState } from "react";
import { loginSecurityUser } from "../api/shortLinksApi";
import { storeSession } from "../api/adminSecurity";
import { ApiError } from "../api/http";
import { toFriendlyErrorMessage } from "../types";
import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../../../shared/components/ui/card";
import { Input } from "../../../shared/components/ui/input";
import { Label } from "../../../shared/components/ui/label";
import { showToast } from "../../../shared/toast";
import { createRecoveryNotice, type RecoveryNotice } from "../../../shared/api/recovery";

type LoginPageProps = {
  onSignedIn: () => void;
};

export function LoginPage({ onSignedIn }: LoginPageProps) {
  const [username, setUsername] = useState("admin");
  const [password, setPassword] = useState("admin");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [failure, setFailure] = useState<RecoveryNotice | null>(null);

  const signIn = async () => {
    if (!username.trim() || !password.trim()) {
      setFailure({ message: "Enter username and password.", retryable: false });
      return;
    }

    setIsSubmitting(true);
    setFailure(null);

    try {
      const result = await loginSecurityUser(username.trim(), password);
      storeSession(result.token, result.user);
      showToast({
        title: "Signed in",
        message: result.user.displayName || result.user.username,
        variant: "success"
      });
      onSignedIn();
    } catch (error) {
      const message = error instanceof ApiError
        ? toFriendlyErrorMessage(error.errorCode, error.message)
        : "Sign in failed.";
      setFailure(createRecoveryNotice(error, message));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Card className="login-panel">
      <CardHeader>
        <p className="eyebrow">Admin identity</p>
        <CardTitle>Sign in</CardTitle>
      </CardHeader>
      <CardContent>
        {failure ? (
          <div className={failure.retryable ? "recovery-banner" : "feedback feedback-error"} role="alert">
            {failure.retryable
              ? `${failure.message} Your credentials are still here; choose Sign in to try again.`
              : failure.message}
          </div>
        ) : null}
        <Label className="field">
          <span className="field-label">Username</span>
          <Input value={username} onChange={(event) => setUsername(event.target.value)} />
        </Label>
        <Label className="field">
          <span className="field-label">Password</span>
          <Input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === "Enter") {
                void signIn();
              }
            }}
          />
        </Label>
        <div className="dialog-actions">
          <Button disabled={isSubmitting} onClick={() => void signIn()}>
            {isSubmitting ? "Signing in" : "Sign in"}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
