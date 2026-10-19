import nodemailer from "nodemailer";
import net from "node:net";
import type SMTPTransport from "nodemailer/lib/smtp-transport";
import { resetEmail, type ResetMail } from "./password-reset";

type MailConfig = {
  SMTP_HOST?: string;
  SMTP_PORT?: string;
  SMTP_USER?: string;
  SMTP_PASSWORD?: string;
  SMTP_FROM?: string;
};

export function openMailSocket(host: string, port: number, callback: (error: Error | null, result: { connection: net.Socket } | false) => void) {
  // Preserve the hostname on the underlying socket. The deployed Workers TLS
  // adapter validates STARTTLS against that hostname, not tls.servername alone.
  // This also avoids Nodemailer's unsupported dns.Resolver/lookup path.
  const connection = net.createConnection({ host, port });
  let settled = false;
  const deadline = setTimeout(() => fail(new Error("Mail connection timed out")), 10_000);
  function fail(error: Error) {
    if (settled) return;
    settled = true;
    clearTimeout(deadline);
    connection.destroy();
    callback(error, false);
  }
  connection.once("error", fail);
  connection.once("connect", () => {
    if (settled) return;
    settled = true;
    clearTimeout(deadline);
    callback(null, { connection });
    connection.removeListener("error", fail);
  });
}

export function createResetSender(config: MailConfig) {
  const host = config.SMTP_HOST?.trim() || "smtp.mail.ovh.net";
  const port = Number(config.SMTP_PORT || "465");
  const user = config.SMTP_USER?.trim();
  const from = resetEmail(config.SMTP_FROM || user);
  if (!/^[a-z0-9.-]+$/i.test(host) || ![465, 587].includes(port) || !user || !config.SMTP_PASSWORD || !from) {
    throw new Error("De OVH-mailrelay is nog niet ingesteld.");
  }

  return async ({ recipient, resetUrl }: ResetMail) => {
    const options: SMTPTransport.Options = {
      host, port,
      getSocket: (_options, callback) => openMailSocket(host, port, callback),
      secure: port === 465,
      requireTLS: true,
      auth: { user, pass: config.SMTP_PASSWORD },
      tls: { servername: host, minVersion: "TLSv1.2", rejectUnauthorized: true },
      connectionTimeout: 10_000,
      greetingTimeout: 10_000,
      socketTimeout: 15_000,
      disableFileAccess: true,
      disableUrlAccess: true,
      logger: false,
      debug: false,
    };
    const transport = nodemailer.createTransport(options);
    try {
      await transport.sendMail({
        from: { name: "Icarus Game", address: from },
        to: recipient,
        subject: "Stel je nieuwe Icarus-wachtwoord in",
        text: `Stel je eigen wachtwoord voor je Icarus-spelaccount in.\n\nOpen deze link om zelf een eerste of nieuw wachtwoord te kiezen:\n${resetUrl}\n\nAlleen deze link is eenmalig en 15 minuten geldig. Je zelfgekozen wachtwoord blijft geldig en gebruik je iedere keer om in te loggen.\nHeb je dit niet aangevraagd? Negeer deze e-mail. Een bestaand wachtwoord is nog niet gewijzigd.\n\nBeheerderswachtwoorden kunnen niet via deze openbare resetfunctie worden hersteld.`,
      });
    } catch (caught) {
      const failure = caught as { code?: unknown; responseCode?: unknown };
      const code = typeof failure.code === "string" && /^(?:E[A-Z_]{1,30}|ERR_[A-Z_]{1,40})$/.test(failure.code) ? failure.code : "MAIL_SEND_FAILED";
      const status = typeof failure.responseCode === "number" && Number.isInteger(failure.responseCode) ? failure.responseCode : 0;
      // Log transport status only, never the SMTP response, address or token.
      console.error("Icarus player mail failed", code, status);
      throw new Error("De instelmail kon niet worden verstuurd. Controleer de OVH-mailverbinding.");
    } finally {
      transport.close();
    }
  };
}
