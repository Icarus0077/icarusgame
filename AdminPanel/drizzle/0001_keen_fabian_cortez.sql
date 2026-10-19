CREATE INDEX `idx_audit_events_created_at` ON `audit_events` (`created_at`);--> statement-breakpoint
CREATE INDEX `idx_card_policies_availability` ON `card_policies` (`availability`);--> statement-breakpoint
CREATE INDEX `idx_game_users_status` ON `game_users` (`status`);--> statement-breakpoint
CREATE INDEX `idx_ip_rules_status_environment` ON `ip_rules` (`status`,`environment`);--> statement-breakpoint
CREATE INDEX `idx_releases_status` ON `releases` (`status`);