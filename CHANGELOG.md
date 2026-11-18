# 1.0.1

Fixed: auto-rest during shearing ignored your bed. When stamina ran low, the mod meditated on the spot instead of using AutoExplore's normal rest, so it never set your carried bed down, recovered slowly, and could leave you passing out mid-shearing. It now uses AutoExplore's own rest (sets a carried bed down, sleeps, packs it back up; otherwise rests as AutoExplore normally would), then carries on shearing. You no longer pass out without using your bed. Also: AI credits updated (Codex and Claude).

修正：毛刈り中の自動休憩でベッドが使われていませんでした。スタミナが減るとAutoExplore本来の休憩ではなくその場で瞑想していたため、持っているベッドを置かず回復が遅く、毛刈り中に倒れることがありました。今後はAutoExplore本来の休憩（持っているベッドを置いて眠り、片付ける。ない場合はAutoExploreの通常の休憩）を行い、その後毛刈りを続けます。ベッドを使わずに倒れることはなくなりました。あわせてAIの表記を更新しました（CodexとClaude）。

修复：剪毛时的自动休息不会使用床。体力不足时，本MOD会原地冥想而不是使用AutoExplore的正常休息，因此不会放下携带的床，恢复很慢，甚至会在剪毛途中晕倒。现在改为使用AutoExplore自己的休息（放下携带的床睡觉，醒后收起；没有床时按AutoExplore的常规方式休息），然后继续剪毛。不会再在不使用床的情况下晕倒。另外更新了AI说明（Codex与Claude）。

# 1.0.0

Initial release: adds map-wide shearing to AutoExplore while shears are selected.
Supports Gather/Combined mode and owned zones when enabled in AutoExplore.

初回公開：ハサミ選択時に、AutoExploreでマップ全体の毛刈りができるようになります。
採取・複合モードに対応。自分の土地ではAutoExplore側の許可設定が必要です。

首次发布：选中剪刀时，为AutoExplore添加全地图自动剪毛。
支持采集和综合模式；在自己的领地使用时需开启AutoExplore的对应选项。
