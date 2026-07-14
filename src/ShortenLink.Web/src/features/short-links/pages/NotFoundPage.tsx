import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "../../../shared/components/ui/card";

type NotFoundPageProps = {
  onBackHome: () => void;
};

export function NotFoundPage({ onBackHome }: NotFoundPageProps) {
  return (
    <Card className="panel-detail">
      <CardHeader>
        <p className="eyebrow">Fallback</p>
        <CardTitle>This route is not part of the demo flow.</CardTitle>
      </CardHeader>
      <CardContent>
      <p className="muted-copy">
        Try creating a new short link from the home screen.
      </p>
      </CardContent>
      <CardFooter>
        <Button onClick={onBackHome}>
          Return to create flow
        </Button>
      </CardFooter>
    </Card>
  );
}
