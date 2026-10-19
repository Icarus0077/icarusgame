CREATE TABLE `game_user_settings` (
	`id` text PRIMARY KEY NOT NULL,
	`user_id` text NOT NULL,
	`game_id` text NOT NULL,
	`can_play` integer DEFAULT true NOT NULL,
	`matchmaking_region` text DEFAULT 'eu-west' NOT NULL,
	`ranked_enabled` integer DEFAULT true NOT NULL,
	`deck_slots` integer DEFAULT 20 NOT NULL,
	`settings_json` text DEFAULT '{}' NOT NULL,
	`updated_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL,
	FOREIGN KEY (`user_id`) REFERENCES `game_users`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
CREATE UNIQUE INDEX `uq_game_user_settings_user_game` ON `game_user_settings` (`user_id`,`game_id`);--> statement-breakpoint
CREATE INDEX `idx_game_user_settings_user_game` ON `game_user_settings` (`user_id`,`game_id`);