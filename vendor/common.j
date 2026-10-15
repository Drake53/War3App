native CreateItem                                       takes integer itemid, real x, real y returns item
native CreateUnit                                       takes player id, integer unitid, real x, real y, real face returns unit
native CreateDestructable                               takes integer objectid, real x, real y, real face, real scale, integer variation returns destructable
native CreateDestructableZ                              takes integer objectid, real x, real y, real z, real face, real scale, integer variation returns destructable
native CreateDeadDestructable                           takes integer objectid, real x, real y, real face, real scale, integer variation returns destructable
native CreateDeadDestructableZ                          takes integer objectid, real x, real y, real z, real face, real scale, integer variation returns destructable

native BlzCreateItemWithSkin                            takes integer itemid, real x, real y, integer skinId returns item
native BlzCreateUnitWithSkin                            takes player id, integer unitid, real x, real y, real face, integer skinId returns unit
native BlzCreateDestructableWithSkin                    takes integer objectid, real x, real y, real face, real scale, integer variation, integer skinId returns destructable
native BlzCreateDestructableZWithSkin                   takes integer objectid, real x, real y, real z, real face, real scale, integer variation, integer skinId returns destructable
native BlzCreateDeadDestructableWithSkin                takes integer objectid, real x, real y, real face, real scale, integer variation, integer skinId returns destructable
native BlzCreateDeadDestructableZWithSkin               takes integer objectid, real x, real y, real z, real face, real scale, integer variation, integer skinId returns destructable

native BlzCreateDestructablePitchRoll                   takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation returns destructable
native BlzCreateDestructableZPitchRoll                  takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation returns destructable
native BlzCreateDeadDestructablePitchRoll               takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation returns destructable
native BlzCreateDeadDestructableZPitchRoll              takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation returns destructable
native BlzCreateDestructableWithSkinPitchRoll           takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation, integer skinId returns destructable
native BlzCreateDestructableZWithSkinPitchRoll          takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation, integer skinId returns destructable
native BlzCreateDeadDestructableWithSkinPitchRoll       takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation, integer skinId returns destructable
native BlzCreateDeadDestructableZWithSkinPitchRoll      takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation, integer skinId returns destructable

native BlzCreateDestructableWithColor                   takes integer objectid, real x, real y, real face, real scale, integer variation, playercolor color returns destructable
native BlzCreateDestructableZWithColor                  takes integer objectid, real x, real y, real z, real face, real scale, integer variation, playercolor color returns destructable
native BlzCreateDeadDestructableWithColor               takes integer objectid, real x, real y, real face, real scale, integer variation, playercolor color returns destructable
native BlzCreateDeadDestructableZWithColor              takes integer objectid, real x, real y, real z, real face, real scale, integer variation, playercolor color returns destructable
native BlzCreateDestructableWithSkinColor               takes integer objectid, real x, real y, real face, real scale, integer variation, integer skinId, playercolor color returns destructable
native BlzCreateDestructableZWithSkinColor              takes integer objectid, real x, real y, real z, real face, real scale, integer variation, integer skinId, playercolor color returns destructable
native BlzCreateDeadDestructableWithSkinColor           takes integer objectid, real x, real y, real face, real scale, integer variation, integer skinId, playercolor color returns destructable
native BlzCreateDeadDestructableZWithSkinColor          takes integer objectid, real x, real y, real z, real face, real scale, integer variation, integer skinId, playercolor color returns destructable
native BlzCreateDestructablePitchRollWithColor          takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation, playercolor color returns destructable
native BlzCreateDestructableZPitchRollWithColor         takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation, playercolor color returns destructable
native BlzCreateDeadDestructablePitchRollWithColor      takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation, playercolor color returns destructable
native BlzCreateDeadDestructableZPitchRollWithColor     takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation, playercolor color returns destructable
native BlzCreateDestructableWithSkinPitchRollColor      takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation, integer skinId, playercolor color returns destructable
native BlzCreateDestructableZWithSkinPitchRollColor     takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation, integer skinId, playercolor color returns destructable
native BlzCreateDeadDestructableWithSkinPitchRollColor  takes integer objectid, real x, real y, real face, real roll, real pitch, real scale, integer variation, integer skinId, playercolor color returns destructable
native BlzCreateDeadDestructableZWithSkinPitchRollColor takes integer objectid, real x, real y, real z, real face, real roll, real pitch, real scale, integer variation, integer skinId, playercolor color returns destructable
