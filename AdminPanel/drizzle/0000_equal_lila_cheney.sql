CREATE TABLE `admin_principals` (
	`id` text PRIMARY KEY NOT NULL,
	`user_id` text NOT NULL,
	`email` text NOT NULL,
	`display_name` text NOT NULL,
	`role` text DEFAULT 'Viewer' NOT NULL,
	`status` text DEFAULT 'active' NOT NULL,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
--> statement-breakpoint
CREATE UNIQUE INDEX `admin_principals_user_id_unique` ON `admin_principals` (`user_id`);--> statement-breakpoint
CREATE UNIQUE INDEX `admin_principals_email_unique` ON `admin_principals` (`email`);--> statement-breakpoint
CREATE TABLE `audit_events` (
	`id` text PRIMARY KEY NOT NULL,
	`actor` text NOT NULL,
	`action` text NOT NULL,
	`target` text NOT NULL,
	`detail` text NOT NULL,
	`severity` text DEFAULT 'info' NOT NULL,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
--> statement-breakpoint
CREATE TABLE `card_policies` (
	`id` text PRIMARY KEY NOT NULL,
	`card_id` text NOT NULL,
	`card_name` text NOT NULL,
	`availability` text DEFAULT 'testers' NOT NULL,
	`audience` text DEFAULT 'Internal QA' NOT NULL,
	`regulation` text DEFAULT 'Standard 2026.08' NOT NULL,
	`effective_from` text DEFAULT CURRENT_TIMESTAMP NOT NULL,
	`effective_to` text,
	`updated_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
--> statement-breakpoint
CREATE UNIQUE INDEX `card_policies_card_id_unique` ON `card_policies` (`card_id`);--> statement-breakpoint
CREATE TABLE `game_users` (
	`id` text PRIMARY KEY NOT NULL,
	`display_name` text NOT NULL,
	`email` text NOT NULL,
	`role` text DEFAULT 'Player' NOT NULL,
	`status` text DEFAULT 'active' NOT NULL,
	`access_group` text DEFAULT 'Public' NOT NULL,
	`last_seen` text DEFAULT CURRENT_TIMESTAMP NOT NULL,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
--> statement-breakpoint
CREATE UNIQUE INDEX `game_users_email_unique` ON `game_users` (`email`);--> statement-breakpoint
CREATE TABLE `ip_rules` (
	`id` text PRIMARY KEY NOT NULL,
	`cidr` text NOT NULL,
	`label` text NOT NULL,
	`scope` text NOT NULL,
	`environment` text NOT NULL,
	`status` text DEFAULT 'active' NOT NULL,
	`expires_at` text,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
--> statement-breakpoint
CREATE TABLE `releases` (
	`id` text PRIMARY KEY NOT NULL,
	`version` text NOT NULL,
	`kind` text NOT NULL,
	`environment` text NOT NULL,
	`status` text DEFAULT 'draft' NOT NULL,
	`notes` text DEFAULT '' NOT NULL,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
--> statement-breakpoint
CREATE TABLE `system_metrics` (
	`key` text PRIMARY KEY NOT NULL,
	`value` integer NOT NULL,
	`updated_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
