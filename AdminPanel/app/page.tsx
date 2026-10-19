import type { Metadata } from "next";
import { getChatGPTUser } from "./chatgpt-auth";
import { AdminConsole } from "./AdminConsole";

export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: "Icarus Control | Game Operations",
  description:
    "Beheer spelers, netwerktoegang, kaartbeschikbaarheid en game-releases vanuit één beveiligde console.",
};

export default async function Home() {
  const user = await getChatGPTUser();

  return (
    <AdminConsole
      viewer={
        user
          ? { name: user.displayName, email: user.email }
          : { name: "Lokale beheerder", email: "preview@project-icarus.local" }
      }
    />
  );
}
